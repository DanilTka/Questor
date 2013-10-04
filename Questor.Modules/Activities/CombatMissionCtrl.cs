// ------------------------------------------------------------------------------
//   <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//     Copyright (c) TheHackerWithin.COM. All Rights Reserved.
//
//     Please look in the accompanying license.htm file for the license that
//     applies to this source code. (a copy can also be found at:
//     http://www.thehackerwithin.com/license.htm)
//   </copyright>
// -------------------------------------------------------------------------------

namespace Questor.Modules.Activities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DirectEve;
    using System.Globalization;
    using Questor.Modules.BackgroundTasks;
    using global::Questor.Modules.Lookup;
    using global::Questor.Modules.Logging;
    using global::Questor.Modules.States;
    using global::Questor.Modules.Combat;
    using Questor.Modules.Caching;

    //using System.Reflection;

    public class CombatMissionCtrl
    {
        private DateTime? _clearPocketTimeout;
        private static int _currentAction;

        private double _lastX;
        private double _lastY;
        private double _lastZ;
        private static List<Actions.Action> _pocketActions;
        private bool _waiting;
        private DateTime _waitingSince;
        private DateTime _moveToNextPocket = DateTime.MaxValue;
        private DateTime _nextCombatMissionCtrlAction = DateTime.UtcNow;
        private int openCargoRetryNumber;
        private int AttemptsToActivateGateTimer;
        private int AttemptsToGetAwayFromGate;

        //private bool _targetNull;

        public long AgentId { get; set; }

        public CombatMissionCtrl()
        {
            //_targetNull = false;
            _pocketActions = new List<Actions.Action>();
        }

        public string Mission { get; set; }

        private void Nextaction()
        {
            // make sure all approach / orbit / align timers are reset (why cant we wait them out in the next action!?)
            Cache.Instance.NextApproachAction = DateTime.UtcNow;
            Cache.Instance.NextOrbit = DateTime.UtcNow;
            Cache.Instance.NextAlign = DateTime.UtcNow;

            // now that we have completed this action revert OpenWrecks to false
            Cache.Instance.OpenWrecks = false;
            Cache.Instance.MissionLoot = false;
            Cache.Instance.normalNav = true;
            Cache.Instance.onlyKillAggro = false;
            _currentAction++;
            return;
        }

        private bool BookmarkPocketForSalvaging()
        {
            if (Settings.Instance.LootEverything && Cache.Instance.UnlootedContainers.Count() > Settings.Instance.MinimumWreckCount)
            {
                List<ModuleCache> tractorBeams = Cache.Instance.Modules.Where(m => m.GroupId == (int)Group.TractorBeam).ToList();
                double RangeToConsiderWrecksDuringLootAll = 0;

                if (tractorBeams.Count > 0)
                {
                    RangeToConsiderWrecksDuringLootAll = tractorBeams.Min(t => t.OptimalRange);
                }
                else
                {
                    RangeToConsiderWrecksDuringLootAll = 20000;
                }

                if (Cache.Instance.UnlootedContainers.Count(w => w.Distance <= RangeToConsiderWrecksDuringLootAll) > 0)
                {
                    //
                    // we probably ought to add some debug spew here 
                    //
                    return false;    
                }

                //
                // if you have loot everything set to on we cant have any need for the pocket bookmarks... can we?!
                //
                return true;
            }

            if (Settings.Instance.CreateSalvageBookmarks)
            {
                // Nothing to loot
                if (Cache.Instance.UnlootedContainers.Count() < Settings.Instance.MinimumWreckCount)
                {
                    // If Settings.Instance.LootEverything is false we may leave behind a lot of unlooted containers.
                    // This scenario only happens when all wrecks are within tractor range and you have a salvager
                    // ( typically only with a Golem ).  Check to see if there are any cargo containers in space.  Cap
                    // boosters may cause an unneeded salvage trip but that is better than leaving millions in loot behind.
                    if (DateTime.UtcNow > Cache.Instance.NextBookmarkPocketAttempt)
                    {
                        Cache.Instance.NextBookmarkPocketAttempt = DateTime.UtcNow.AddSeconds(Time.Instance.BookmarkPocketRetryDelay_seconds);
                        if (!Settings.Instance.LootEverything && Cache.Instance.Containers.Count() < Settings.Instance.MinimumWreckCount)
                        {
                            Logging.Log("CombatMissionCtrl", "No bookmark created because the pocket has [" + Cache.Instance.Containers.Count() + "] wrecks/containers and the minimum is [" + Settings.Instance.MinimumWreckCount + "]", Logging.Teal);
                            return true;
                        }

                        if (Settings.Instance.LootEverything)
                        {
                            Logging.Log("CombatMissionCtrl", "No bookmark created because the pocket has [" + Cache.Instance.UnlootedContainers.Count() + "] wrecks/containers and the minimum is [" + Settings.Instance.MinimumWreckCount + "]", Logging.Teal);
                            return true;
                        }

                        return false;
                    }

                    return false;
                }

                // Do we already have a bookmark?
                List<DirectBookmark> bookmarks = Cache.Instance.BookmarksByLabel(Settings.Instance.BookmarkPrefix + " ");
                DirectBookmark bookmark = bookmarks.FirstOrDefault(b => Cache.Instance.DistanceFromMe(b.X ?? 0, b.Y ?? 0, b.Z ?? 0) < (int)Distances.OnGridWithMe);
                if (bookmark != null)
                {
                    Logging.Log("CombatMissionCtrl", "salvaging bookmark forthis pocket is done [" + bookmark.Title + "]", Logging.Teal);
                    return true;
                }

                // No, create a bookmark
                string label = string.Format("{0} {1:HHmm}", Settings.Instance.BookmarkPrefix, DateTime.UtcNow);
                Logging.Log("CombatMissionCtrl", "Bookmarking pocket for salvaging [" + label + "]", Logging.Teal);
                Cache.Instance.CreateBookmark(label);
                return false;
            }

            return true;
        }

        private void AddPriorityKillTargetsAndMoveIntoRangeAsNeeded(EntityCache target, double range, int targetedby, bool MoveShip)
        {
            if (target != null)
            {
                // Reset timeout
                _clearPocketTimeout = null;
                
                // Combat.cs will Lock target if it is within weapons range (especially after we make it a PrimaryWeapons or Drone prioritytarget!)
                if (target.Distance < range)
                {
                    //panic handles adding any priority targets and combat will prefer to kill any priority targets
                    if (targetedby == 0 && DateTime.UtcNow > Cache.Instance.NextReload)
                    {
                        if (!Combat.ReloadAll(target)) return;
                    }

                    //Adds the target we want to kill to the priority list so that combat.cs will kill it (especially if it is an LCO this is important)
                    if (!target.IsNPCFrigate)
                    {
                        Cache.Instance.AddPrimaryWeaponPriorityTargets(new[] {target}, PrimaryWeaponPriority.PriorityKillTarget, "CombatMissionCtrl." + _pocketActions[_currentAction]);
                    }

                    if (Settings.Instance.DronesKillHighValueTargets || target.IsNPCFrigate) //killing of normal frigates and E-war frigates is handled by combat as needed, this is for killing big stuff w or drones (Dominix? ishtar?)
                    {
                        Cache.Instance.AddDronePriorityTargets(new[] {target}, DronePriority.LowPriorityTarget, "CombatMissionCtrl." + _pocketActions[_currentAction]);
                    }
                }

                if (MoveShip)
                {
                    NavigateOnGrid.NavigateIntoRange(target, "CombatMissionCtrl." + _pocketActions[_currentAction], true);
                }

                if (target.Distance > range) //target is not in range...
                {
                    if (DateTime.UtcNow > Cache.Instance.NextReload)
                    {
                        //Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction] ,"ReloadAll: Reload weapons",Logging.teal);
                        if (!Combat.ReloadAll(target)) return;
                    }
                }
            }

            return;
        }

        private void DoneAction()
        {
            // Tell the drones module to retract drones
            Cache.Instance.IsMissionPocketDone = true;
            Cache.Instance.UseDrones = Settings.Instance.UseDrones;

            // We do not switch to "done" status if we still have drones out
            if (Cache.Instance.ActiveDrones.Any()) return;

            // Add bookmark (before we're done)
            if (Settings.Instance.CreateSalvageBookmarks)
            {
                if (!BookmarkPocketForSalvaging()) return;
            }

            //
            // we are ready and can set the "done" State. 
            //
            Cache.Instance.CurrentlyShouldBeSalvaging = false;
            _States.CurrentCombatMissionCtrlState = CombatMissionCtrlState.Done;
            return;
        }

        private void LogWhatIsOnGridAction(Actions.Action action)
        {

            Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Log Entities on Grid.", Logging.Teal);
            if (!Statistics.EntityStatistics(Cache.Instance.Entities)) return;
            Nextaction();
            return;
        }

        private void ActivateAction(Actions.Action action)
        {
            bool optional;
            if (!bool.TryParse(action.GetParameterValue("optional"), out optional))
            {
                optional = false;
            }

            string target = action.GetParameterValue("target");

            // No parameter? Although we should not really allow it, assume its the acceleration gate :)
            if (string.IsNullOrEmpty(target))
            {
                target = "Acceleration Gate";
            }

            IEnumerable<EntityCache> targets = Cache.Instance.EntitiesByName(target).ToList();
            if (!targets.Any())
            {
                if (!_waiting)
                {
                    Logging.Log("CombatMissionCtrl", "Activate: Can't find [" + target + "] to activate! Waiting 30 seconds before giving up", Logging.Teal);
                    _waitingSince = DateTime.UtcNow;
                    _waiting = true;
                }
                else if (_waiting)
                {
                    if (DateTime.UtcNow.Subtract(_waitingSince).TotalSeconds > Time.Instance.NoGateFoundRetryDelay_seconds)
                    {
                        Logging.Log("CombatMissionCtrl",
                                    "Activate: After 30 seconds of waiting the gate is still not on grid: CombatMissionCtrlState.Error",
                                    Logging.Teal);
                        if (optional) //if this action has the optional paramater defined as true then we are done if we cant find the gate
                        {
                            DoneAction();
                        }
                        else
                        {
                            _States.CurrentCombatMissionCtrlState = CombatMissionCtrlState.Error;
                        }
                    }
                }
                return;
            }

            //if (closest.Distance <= (int)Distance.CloseToGateActivationRange) // if your distance is less than the 'close enough' range, default is 7000 meters
            EntityCache closest = targets.OrderBy(t => t.Distance).FirstOrDefault();
            
            if (closest != null)
            {
                if (closest.Distance <= (int)Distances.GateActivationRange)
                {
                    if (Settings.Instance.DebugActivateGate) Logging.Log("CombatMissionCtrl", "if (closest.Distance [" + closest.Distance + "] <= (int)Distances.GateActivationRange [" + (int)Distances.GateActivationRange + "])", Logging.Green);

                    // Tell the drones module to retract drones
                    Cache.Instance.IsMissionPocketDone = true;

                    // We cant activate if we have drones out
                    if (Cache.Instance.ActiveDrones.Any())
                    {
                        if (Settings.Instance.DebugActivateGate) Logging.Log("CombatMissionCtrl", "if (Cache.Instance.ActiveDrones.Any())", Logging.Green);
                        return;
                    }

                    //
                    // this is a bad idea for a speed tank, we ought to somehow cache the object they are orbiting/approaching, etc
                    // this seemingly slowed down the exit from certain missions for me for 2-3min as it had a command to orbit some random object
                    // after the "done" command
                    //
                    if (closest.Distance < -10100)
                    {
                        if (Settings.Instance.DebugActivateGate) Logging.Log("CombatMissionCtrl", "if (closest.Distance < -10100)", Logging.Green);

                        AttemptsToGetAwayFromGate++;
                        if (AttemptsToGetAwayFromGate > 30)
                        {
                            if (DateTime.UtcNow > Cache.Instance.NextOrbit)
                            {
                                closest.Orbit(1000);
                                Logging.Log("CombatMissionCtrl", "Activate: We are too close to [" + closest.Name + "] Initiating orbit", Logging.Orange);
                            }

                            return;
                        }
                    }

                    if (Settings.Instance.DebugActivateGate) Logging.Log("CombatMissionCtrl", "if (closest.Distance >= -10100)", Logging.Green);

                    // Add bookmark (before we activate)
                    if (Settings.Instance.CreateSalvageBookmarks)
                    {
                        BookmarkPocketForSalvaging();
                    }

                    if (Settings.Instance.DebugActivateGate) Logging.Log("CombatMissionCtrl", "Activate: Reload before moving to next pocket", Logging.Teal);
                    if (!Combat.ReloadAll(Cache.Instance.MyShipEntity)) return;
                    if (Settings.Instance.DebugActivateGate) Logging.Log("CombatMissionCtrl", "Activate: Done reloading", Logging.Teal);
                    AttemptsToActivateGateTimer++;

                    if (DateTime.UtcNow > Cache.Instance.NextActivateAction || AttemptsToActivateGateTimer > 30)
                    {
                        Logging.Log("CombatMissionCtrl", "Activate: [" + closest.Name + "] Move to next pocket after reload command and change state to 'NextPocket'", Logging.Green);
                        closest.Activate();
                        AttemptsToActivateGateTimer = 0;
                        // Do not change actions, if NextPocket gets a timeout (>2 mins) then it reverts to the last action
                        _moveToNextPocket = DateTime.UtcNow;
                        _States.CurrentCombatMissionCtrlState = CombatMissionCtrlState.NextPocket;
                    }

                    if (Settings.Instance.DebugActivateGate) Logging.Log("CombatMissionCtrl", "------------------", Logging.Green);
                    return;
                }

                AttemptsToActivateGateTimer = 0;
                AttemptsToGetAwayFromGate = 0;

                if (closest.Distance < (int)Distances.WarptoDistance) //else if (closest.Distance < (int)Distances.WarptoDistance) //if we are inside warpto distance then approach
                {
                    if (Settings.Instance.DebugActivateGate) Logging.Log("CombatMissionCtrl", "if (closest.Distance < (int)Distances.WarptoDistance)", Logging.Green);

                    // Move to the target
                    if (DateTime.UtcNow > Cache.Instance.NextApproachAction)
                    {
                        if (Cache.Instance.IsOrbiting(closest.Id) || Cache.Instance.Approaching == null || Cache.Instance.Approaching.Id != closest.Id || Cache.Instance.MyShipEntity.Velocity < 100)
                        {
                            Logging.Log("CombatMissionCtrl.Activate", "Approaching target [" + closest.Name + "][ID: " + Cache.Instance.MaskedID(closest.Id) + "][" + Math.Round(closest.Distance / 1000, 0) + "k away]", Logging.Teal);
                            closest.Approach();
                            return;
                        }

                        if (Settings.Instance.DebugActivateGate) Logging.Log("CombatMissionCtrl", "Cache.Instance.IsOrbiting [" + Cache.Instance.IsOrbiting(closest.Id) + "] Cache.Instance.MyShip.Velocity [" + Math.Round(Cache.Instance.MyShipEntity.Velocity,0) + "m/s]", Logging.Green);
                        if (Settings.Instance.DebugActivateGate) if (Cache.Instance.Approaching != null) Logging.Log("CombatMissionCtrl", "Cache.Instance.Approaching.Id [" + Cache.Instance.Approaching.Id + "][closest.Id: " + closest.Id + "]", Logging.Green);
                        if (Settings.Instance.DebugActivateGate) Logging.Log("CombatMissionCtrl", "------------------", Logging.Green);
                        return;
                    }

                    if (Cache.Instance.IsOrbiting(closest.Id) || Cache.Instance.Approaching == null || Cache.Instance.Approaching.Id != closest.Id)
                    {
                        Logging.Log("CombatMissionCtrl", "Activate: Delaying approach for: [" + Math.Round(Cache.Instance.NextApproachAction.Subtract(DateTime.UtcNow).TotalSeconds, 0) + "] seconds", Logging.Teal);
                        return;
                    }

                    if (Settings.Instance.DebugActivateGate) Logging.Log("CombatMissionCtrl", "------------------", Logging.Green);
                    return;
                }

                if (closest.Distance > (int)Distances.WarptoDistance)//we must be outside warpto distance, but we are likely in a deadspace so align to the target
                {
                    // We cant warp if we have drones out - but we are aligning not warping so we do not care
                    //if (Cache.Instance.ActiveDrones.Count() > 0)
                    //    return;

                    if (DateTime.UtcNow > Cache.Instance.NextAlign)
                    {
                        // Only happens if we are asked to Activate something that is outside Distance.CloseToGateActivationRange (default is: 6k)
                        Logging.Log("CombatMissionCtrl", "Activate: AlignTo: [" + closest.Name + "] This only happens if we are asked to Activate something that is outside [" + Distances.CloseToGateActivationRange + "]", Logging.Teal);
                        closest.AlignTo();
                        return;
                    }

                    Logging.Log("CombatMissionCtrl", "Activate: Unable to align: Next Align in [" + Cache.Instance.NextAlign.Subtract(DateTime.UtcNow).TotalSeconds + "] seconds", Logging.Teal);
                    return;
                }

                Logging.Log("CombatMissionCtrl", "Activate: Error: [" + closest.Name + "] at [" + closest.Distance + "] is not within jump distance, within warpable distance or outside warpable distance, (!!!), retrying action.", Logging.Teal);
            }

            return;
        }

        private void ClearAggroAction(Actions.Action action)
        {
            if (!Cache.Instance.NormalApproach) Cache.Instance.NormalApproach = true;

            // Get lowest range
            int DistanceToClear;
            if (!int.TryParse(action.GetParameterValue("distance"), out DistanceToClear))
            {
                DistanceToClear = (int)Distances.OnGridWithMe;
            }

            if (DistanceToClear != 0 && DistanceToClear != -2147483648 && DistanceToClear != 2147483647)
            {
                DistanceToClear = (int)Distances.OnGridWithMe;
            }

            if (Cache.Instance.GetBestTarget(DistanceToClear, false, "combat", Cache.Instance.combatTargets.Where(t => t.IsTargetedBy && !t.IsIgnored && !t.IsSentry || (t.IsSentry && Settings.Instance.KillSentries)).ToList())
                || Cache.Instance.GetBestDroneTarget(DistanceToClear, false, "Drones", Cache.Instance.combatTargets.Where(t => t.IsTargetedBy && !t.IsIgnored && !t.IsSentry || (t.IsSentry && Settings.Instance.KillSentries)).ToList()))
                _clearPocketTimeout = null;

            // Do we have a timeout?  No, set it to now + 5 seconds
            if (!_clearPocketTimeout.HasValue) _clearPocketTimeout = DateTime.UtcNow.AddSeconds(5);

            // Are we in timeout?
            if (DateTime.UtcNow < _clearPocketTimeout.Value) return;

            // We have cleared the Pocket, perform the next action \o/ - reset the timers that we had set for actions...
            Nextaction();

            // Reset timeout
            _clearPocketTimeout = null;
        }

        private void ClearPocketAction(Actions.Action action)
        {
            if (!Cache.Instance.NormalApproach)
            {
                Cache.Instance.NormalApproach = true;
            }

            // Get lowest range
            int DistanceToClear;
            if (!int.TryParse(action.GetParameterValue("distance"), out DistanceToClear))
            {
                DistanceToClear = (int)Cache.Instance.MaxRange;
            }

            if (DistanceToClear != 0 && DistanceToClear != -2147483648 && DistanceToClear != 2147483647)
            {
                DistanceToClear = (int)Distances.OnGridWithMe;
            }

            //panic handles adding any priority targets and combat will prefer to kill any priority targets

            //If the closest target is out side of our max range, combat cant target, which means GetBest cant return true, so we are going to try and use potentialCombatTargets instead
            if (Cache.Instance.potentialCombatTargets.Any(t => !t.IsIgnored && !t.IsSentry || (t.IsSentry && Settings.Instance.KillSentries)))
            {
                //we may be too far out of range of the closest target to get combat to kick in, lets move us into range here
                EntityCache ClosestPotentialCombatTarget = Cache.Instance.potentialCombatTargets.Where(e => !e.IsSentry || (e.IsSentry && Settings.Instance.KillSentries)).OrderBy(t => t.Nearest5kDistance).FirstOrDefault();
                if (ClosestPotentialCombatTarget == null)
                {
                    ClosestPotentialCombatTarget = Cache.Instance.potentialCombatTargets.FirstOrDefault();
                }

                if (ClosestPotentialCombatTarget != null && ClosestPotentialCombatTarget.Distance > Cache.Instance.MaxRange)
                {
                    if (!Cache.Instance.IsApproachingOrOrbiting(ClosestPotentialCombatTarget.Id))
                    {
                        NavigateOnGrid.NavigateIntoRange(ClosestPotentialCombatTarget, "combatMissionControl", true);
                    }
                }
                else
                {
                    Cache.Instance.GetBestTarget(DistanceToClear, false, "combat");
                    if (Cache.Instance.UseDrones)
                        Cache.Instance.GetBestDroneTarget(DistanceToClear, false, "Drones");
                }
                _clearPocketTimeout = null;
            }
            //Cache.Instance.AddPrimaryWeaponPriorityTargets(Cache.Instance.potentialCombatTargets.Where(t => targetNames.Contains(t.Name)).OrderBy(t => t.Distance).ToList(), PrimaryWeaponPriority.PriorityKillTarget, "CombatMissionCtrl.KillClosestByName");


            // Do we have a timeout?  No, set it to now + 5 seconds
            if (!_clearPocketTimeout.HasValue) _clearPocketTimeout = DateTime.UtcNow.AddSeconds(5);

            // Are we in timeout?
            if (DateTime.UtcNow < _clearPocketTimeout.Value) return;

            // We have cleared the Pocket, perform the next action \o/ - reset the timers that we had set for actions...
            Nextaction();

            // Reset timeout
            _clearPocketTimeout = null;
            return;
        }

        private void ClearWithinWeaponsRangeOnlyAction(Actions.Action action)
        {
            // Get lowest range
            int DistanceToClear;
            if (!int.TryParse(action.GetParameterValue("distance"), out DistanceToClear))
            {
                DistanceToClear = (int)Cache.Instance.MaxRange;
            }

            if (DistanceToClear != 0 && DistanceToClear != -2147483648 && DistanceToClear != 2147483647)
            {
                DistanceToClear = (int)Distances.OnGridWithMe;
            }            

            //
            // note this WILL clear sentries within the range given... it does NOT respect the KillSentries setting. 75% of the time this wont matter as sentries will be outside the range
            //

            // Target
            if (Cache.Instance.GetBestTarget(DistanceToClear, false, "combat") || Cache.Instance.GetBestDroneTarget(DistanceToClear, false, "Drones"))
                _clearPocketTimeout = null;

            // Do we have a timeout?  No, set it to now + 5 seconds
            if (!_clearPocketTimeout.HasValue)
            {
                _clearPocketTimeout = DateTime.UtcNow.AddSeconds(5);
            }

            // Are we in timeout?
            if (DateTime.UtcNow < _clearPocketTimeout.Value)
            {
                return;
            }

            Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "is complete: no more targets in weapons range", Logging.Teal);
            Nextaction();

            // Reset timeout
            _clearPocketTimeout = null;
            return;
        }

        private void ClearWithinWeaponsRangeWithAggroOnlyAction(Actions.Action action)
        {
            if (Cache.Instance.NormalApproach)
            {
                Cache.Instance.NormalApproach = false;
            }

            // Get lowest range
            int DistanceToClear;
            if (!int.TryParse(action.GetParameterValue("distance"), out DistanceToClear))
            {
                DistanceToClear = (int)Cache.Instance.MaxRange;
            }

            if (DistanceToClear != 0 && DistanceToClear != -2147483648 && DistanceToClear != 2147483647)
            {
                DistanceToClear = (int)Distances.OnGridWithMe;
            }        

            //
            // the important bit is here... Adds target to the PrimaryWeapon or Drone Priority Target Lists so that they get killed (we basically wait for combat.cs to do that before proceeding)
            //
            if (Cache.Instance.GetBestTarget(DistanceToClear, false, "combat", Cache.Instance.combatTargets.Where(t => t.IsTargetedBy).ToList())
                    || Cache.Instance.GetBestDroneTarget(DistanceToClear, false, "Drones", Cache.Instance.combatTargets.Where(t => t.IsTargetedBy).ToList()))
                _clearPocketTimeout = null;

            // Do we have a timeout?  No, set it to now + 5 seconds
            if (!_clearPocketTimeout.HasValue)
            {
                _clearPocketTimeout = DateTime.UtcNow.AddSeconds(5);
            }

            // Are we in timeout?
            if (DateTime.UtcNow < _clearPocketTimeout.Value)
            {
                return;
            }

            Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "is complete: no more targets that are targeting us", Logging.Teal);
            Nextaction();

            // Reset timeout
            _clearPocketTimeout = null;
            return;
        }

        private void OrbitEntityAction(Actions.Action action)
        {
            if (Cache.Instance.NormalApproach)
            {
                Cache.Instance.NormalApproach = false;
            }

            Cache.Instance.normalNav = false;

            string target = action.GetParameterValue("target");

            bool notTheClosest;
            if (!bool.TryParse(action.GetParameterValue("notclosest"), out notTheClosest))
            {
                notTheClosest = false;
            }

            // No parameter? Although we should not really allow it, assume its the acceleration gate :)
            if (string.IsNullOrEmpty(target))
            {
                Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "No Entity Specified to orbit: skipping OrbitEntity Action", Logging.Teal);
                Nextaction();
                return;
            }
               
            IEnumerable<EntityCache> targets = Cache.Instance.EntitiesByNamePart(target).ToList();
            if (!targets.Any())
            {
                // Unlike activate, no target just means next action
                _currentAction++;
                return;
            }

            EntityCache closest = targets.OrderBy(t => t.Distance).FirstOrDefault();

            if (notTheClosest)
            {
                closest = targets.OrderByDescending(t => t.Distance).FirstOrDefault();
            }

            if (closest != null)
            {
                // Move to the target
                Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Setting [" + closest.Name + "][ID: " + Cache.Instance.MaskedID(closest.Id) + "][" + Math.Round(closest.Distance / 1000, 0) + "k away as the Orbit Target]", Logging.Teal);
                closest.Orbit(Cache.Instance.OrbitDistance);    
            }

            Nextaction();
            return;
        }

        private void MoveToBackgroundAction(Actions.Action action)
        {
            if (DateTime.UtcNow < _nextCombatMissionCtrlAction)
                return;

            if (Cache.Instance.NormalApproach)
            {
                Cache.Instance.NormalApproach = false;
            }

            Cache.Instance.normalNav = false;

            int DistanceToApproach;
            if (!int.TryParse(action.GetParameterValue("distance"), out DistanceToApproach))
            {
                DistanceToApproach = (int)Distances.GateActivationRange;
            }

            string target = action.GetParameterValue("target");

            // No parameter? Although we should not really allow it, assume its the acceleration gate :)
            if (string.IsNullOrEmpty(target))
            {
                target = "Acceleration Gate";
            }

            IEnumerable<EntityCache> targets = Cache.Instance.EntitiesByName(target).ToList();
            if (!targets.Any())
            {
                // Unlike activate, no target just means next action
                _currentAction++;
                return;
            }

            EntityCache closest = targets.OrderBy(t => t.Distance).FirstOrDefault();

            if (closest != null)
            {
                // Move to the target
                Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Approaching target [" + closest.Name + "][ID: " + Cache.Instance.MaskedID(closest.Id) + "][" + Math.Round(closest.Distance / 1000, 0) + "k away]", Logging.Teal);
                closest.Approach(DistanceToApproach);
                _nextCombatMissionCtrlAction = DateTime.UtcNow.AddSeconds(5);
            }

            Nextaction();
            return;
        }

        private void MoveToAction(Actions.Action action)
        {
            if (DateTime.UtcNow < _nextCombatMissionCtrlAction)
                return;

            if (Cache.Instance.NormalApproach)
            {
                Cache.Instance.NormalApproach = false;
            }

            Cache.Instance.normalNav = false;

            string target = action.GetParameterValue("target");

            // No parameter? Although we should not really allow it, assume its the acceleration gate :)
            if (string.IsNullOrEmpty(target))
            {
                target = "Acceleration Gate";
            }

            int DistanceToApproach;
            if (!int.TryParse(action.GetParameterValue("distance"), out DistanceToApproach))
            {
                DistanceToApproach = (int)Distances.GateActivationRange;
            }

            bool stopWhenTargeted;
            if (!bool.TryParse(action.GetParameterValue("StopWhenTargeted"), out stopWhenTargeted))
            {
                stopWhenTargeted = false;
            }

            bool stopWhenAggressed;
            if (!bool.TryParse(action.GetParameterValue("StopWhenAggressed"), out stopWhenAggressed))
            {
                stopWhenAggressed = false;
            }

            IEnumerable<EntityCache> targets = Cache.Instance.EntitiesByName(target).ToList();
            if (!targets.Any())
            {
                Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "no entities found named [" + target + "] proceeding to next action", Logging.Teal);
                Nextaction();
                return;
            }

            EntityCache closest = targets.OrderBy(t => t.Distance).FirstOrDefault();

            if (closest != null)
            {
                if (stopWhenTargeted)
                {
                    if (Cache.Instance.TargetedBy != null && Cache.Instance.TargetedBy.Any())
                    {
                        if (Cache.Instance.Approaching != null)
                        {
                            if (Cache.Instance.MyShipEntity.Velocity != 0) Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.CmdStopShip);
                            Cache.Instance.Approaching = null;
                            Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Stop ship, we have been targeted and are [" + DistanceToApproach + "] from [ID: " + closest.Name + "][" + Math.Round(closest.Distance / 1000, 0) + "k away]", Logging.Teal);
                        }
                    }
                }

                if (stopWhenAggressed)
                {
                    if (Cache.Instance.Aggressed.Any(t => !t.IsSentry))
                    {
                        if (Cache.Instance.Approaching != null)
                        {
                            if (Cache.Instance.MyShipEntity.Velocity != 0) Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.CmdStopShip);
                            Cache.Instance.Approaching = null;
                            Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Stop ship, we have been targeted and are [" + DistanceToApproach + "] from [ID: " + closest.Name + "][" + Math.Round(closest.Distance / 1000, 0) + "k away]", Logging.Teal);
                        }
                    }
                }

                if (closest.Distance < DistanceToApproach) // if we are inside the range that we are supposed to approach assume we are done
                {
                    Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "We are [" + Math.Round(closest.Distance, 0) + "] from a [" + target + "] we do not need to go any further", Logging.Teal);
                    Nextaction();

                    if (Cache.Instance.Approaching != null)
                    {
                        if (Cache.Instance.MyShipEntity.Velocity != 0) Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.CmdStopShip);
                        Cache.Instance.Approaching = null;
                        Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Stop ship, we are [" + DistanceToApproach + "] from [ID: " + closest.Name + "][" + Math.Round(closest.Distance / 1000, 0) + "k away]", Logging.Teal);
                    }

                    //if (Settings.Instance.SpeedTank)
                    //{
                    //    //this should at least keep speed tanked ships from going poof if a mission XML uses moveto
                    //    closest.Orbit(Cache.Instance.OrbitDistance);
                    //    Logging.Log("CombatMissionCtrl","MoveTo: Initiating orbit after reaching target")
                    //}
                    return;
                }

                if (closest.Distance < (int)Distances.WarptoDistance) // if we are inside warpto range you need to approach (you cant warp from here)
                {
                    if (Settings.Instance.DebugMoveTo) Logging.Log("CombatMissionCtrl.MoveTo", "if (closest.Distance < (int)Distances.WarptoDistance)] -  NextApproachAction [" + Cache.Instance.NextApproachAction + "]", Logging.Teal);

                    // Move to the target

                    if (Settings.Instance.DebugMoveTo) if (Cache.Instance.Approaching == null) Logging.Log("CombatMissionCtrl.MoveTo", "if (Cache.Instance.Approaching == null)", Logging.Teal);
                    if (Settings.Instance.DebugMoveTo) if (Cache.Instance.Approaching != null) Logging.Log("CombatMissionCtrl.MoveTo", "Cache.Instance.Approaching.Id [" + Cache.Instance.Approaching.Id + "]", Logging.Teal);
                    if (Cache.Instance.Approaching == null || Cache.Instance.Approaching.Id != closest.Id || Cache.Instance.MyShipEntity.Velocity < 100)
                    {
                        Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Approaching target [" + closest.Name + "][ID: " + Cache.Instance.MaskedID(closest.Id) + "][" + Math.Round(closest.Distance / 1000, 0) + "k away]", Logging.Teal);
                        closest.Approach();
                        _nextCombatMissionCtrlAction = DateTime.UtcNow.AddSeconds(5);
                        return;
                    }
                    if (Settings.Instance.DebugMoveTo) if (Cache.Instance.Approaching != null) Logging.Log("CombatMissionCtrl.MoveTo", "-----------", Logging.Teal);
                    return;
                }

                if (DateTime.UtcNow > Cache.Instance.NextAlign)
                {
                    // Probably never happens
                    Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Aligning to target [" + closest.Name + "][ID: " + Cache.Instance.MaskedID(closest.Id) + "][" + Math.Round(closest.Distance / 1000, 0) + "k away]", Logging.Teal);
                    closest.AlignTo();
                    _nextCombatMissionCtrlAction = DateTime.UtcNow.AddSeconds(5);
                    return;
                }

                if (Settings.Instance.DebugMoveTo) Logging.Log("CombatMissionCtrl.MoveTo", "Nothing to do. Next Approach [" + Cache.Instance.NextApproachAction + " ] NextAlign [" + Cache.Instance.NextAlign + "]", Logging.Teal);
            }

            return;
        }

        private void WaitUntilTargeted(Actions.Action action)
        {
            IEnumerable<EntityCache> targetedBy = Cache.Instance.TargetedBy;
            if (targetedBy != null && targetedBy.Any())
            {
                Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "We have been targeted!", Logging.Teal);

                // We have been locked, go go go ;)
                _waiting = false;
                Nextaction();
                return;
            }

            // Default timeout is 30 seconds
            int timeout;
            if (!int.TryParse(action.GetParameterValue("timeout"), out timeout))
            {
                timeout = 30;
            }

            if (_waiting)
            {
                if (DateTime.UtcNow.Subtract(_waitingSince).TotalSeconds < timeout)
                {
                    return;
                }

                Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Nothing targeted us within [ " + timeout + "sec]!", Logging.Teal);

                // Nothing has targeted us in the specified timeout
                _waiting = false;
                Nextaction();
                return;
            }

            // Start waiting
            _waiting = true;
            _waitingSince = DateTime.UtcNow;
            return;
        }

        private void DebuggingWait(Actions.Action action)
        {
            // Default timeout is 1200 seconds
            int timeout;
            if (!int.TryParse(action.GetParameterValue("timeout"), out timeout))
            {
                timeout = 1200;
            }

            if (_waiting)
            {
                if (DateTime.UtcNow.Subtract(_waitingSince).TotalSeconds < timeout)
                {
                    return;
                }

                Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Nothing targeted us within [ " + timeout + "sec]!", Logging.Teal);

                // Nothing has targeted us in the specified timeout
                _waiting = false;
                Nextaction();
                return;
            }

            // Start waiting
            _waiting = true;
            _waitingSince = DateTime.UtcNow;
            return;
        }

        private void AggroOnlyAction(Actions.Action action)
        {
            if (Cache.Instance.NormalApproach)
            {
                Cache.Instance.NormalApproach = false;
            }

            // Get lowest range
            int DistanceToClear;
            if (!int.TryParse(action.GetParameterValue("distance"), out DistanceToClear))
            {
                DistanceToClear = (int)Distances.OnGridWithMe;
            }

            if (DistanceToClear != 0 && DistanceToClear != -2147483648 && DistanceToClear != 2147483647)
            {
                DistanceToClear = (int)Distances.OnGridWithMe;
            }

            //
            // the important bit is here... Adds target to the PrimaryWeapon or Drone Priority Target Lists so that they get killed (we basically wait for combat.cs to do that before proceeding)
            //
            if (Cache.Instance.GetBestTarget(DistanceToClear, false, "combat", Cache.Instance.combatTargets.Where(t => t.IsTargetedBy).ToList())
                    || Cache.Instance.GetBestDroneTarget(DistanceToClear, false, "Drones", Cache.Instance.combatTargets.Where(t => t.IsTargetedBy).ToList()))
                _clearPocketTimeout = null;

            // Do we have a timeout?  No, set it to now + 5 seconds
            if (!_clearPocketTimeout.HasValue)
            {
                _clearPocketTimeout = DateTime.UtcNow.AddSeconds(5);
            }

            // Are we in timeout?
            if (DateTime.UtcNow < _clearPocketTimeout.Value)
            {
                return;
            }

            Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "is complete: no more targets that are targeting us", Logging.Teal);
            Nextaction();

            // Reset timeout
            _clearPocketTimeout = null;
            return;
        }

        private void KillAction(Actions.Action action)
        {
            if (Cache.Instance.NormalApproach) Cache.Instance.NormalApproach = false;

            bool ignoreAttackers;
            if (!bool.TryParse(action.GetParameterValue("ignoreattackers"), out ignoreAttackers))
            {
                ignoreAttackers = false;   
            }

            bool breakOnAttackers;
            if (!bool.TryParse(action.GetParameterValue("breakonattackers"), out breakOnAttackers))
            {
                breakOnAttackers = false;
            }

            bool notTheClosest;
            if (!bool.TryParse(action.GetParameterValue("notclosest"), out notTheClosest))
            {
                notTheClosest = false;
            }

            int numberToIgnore;
            if (!int.TryParse(action.GetParameterValue("numbertoignore"), out numberToIgnore))
            {
                numberToIgnore = 0;
            }

            List<string> targetNames = action.GetParameterValues("target");

            // No parameter? Ignore kill action
            if (targetNames.Count == 0)
            {
                Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "No targets defined in kill action!", Logging.Teal);
                Nextaction();
                return;
            }

            IEnumerable<EntityCache> killTargets = Cache.Instance.Entities.Where(e => e.IsOnGridWithMe && targetNames.Contains(e.Name)).OrderBy(t => t.Distance);

            if (!killTargets.Any() || killTargets.Count() <= numberToIgnore)
            {
                Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "All targets killed " + targetNames.Aggregate((current, next) => current + "[" + next + "]"), Logging.Teal);

                // We killed it/them !?!?!? :)
                Cache.Instance.IgnoreTargets.RemoveWhere(targetNames.Contains);
                Nextaction();
                return;
            }

            if (ignoreAttackers)
            {
                foreach (EntityCache target in Cache.Instance.potentialCombatTargets.Where(e => !targetNames.Contains(e.Name)))
                {
                    if (target.IsTargetedBy && target.IsAttacking)
                    {
                        Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Ignoring [" + target.Name + "][ID: " + Cache.Instance.MaskedID(target.Id) + "][" + Math.Round(target.Distance / 1000, 0) + "k away] due to ignoreAttackers paramater", Logging.Teal);
                        Cache.Instance.IgnoreTargets.Add(target.Name.Trim());
                    }    
                }
            }

            if (breakOnAttackers && Cache.Instance.TargetedBy.Count(t => (!t.IsSentry || Settings.Instance.KillSentries) && !t.IsIgnored) > killTargets.Count(e => e.IsTargetedBy))
            {
                // We are being attacked, break the kill order
                if (Cache.Instance.RemovePrimaryWeaponPriorityTargets(killTargets)) Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Breaking off kill order, new spawn has arrived!", Logging.Teal);
                targetNames.ForEach(t => Cache.Instance.IgnoreTargets.Add(t));
                
                if (killTargets.Any())
                {
                    foreach (EntityCache killTarget in killTargets.Where(e => targetNames.Contains(Cache.Instance.PreferredPrimaryWeaponTarget.Name)))
                    {
                        if (Cache.Instance.PreferredPrimaryWeaponTarget != null)
                        {
                            if (killTarget.Name.Contains(Cache.Instance.PreferredPrimaryWeaponTarget.Name))
                            {
                                Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "if (killTarget.Name.Contains(Cache.Instance.PreferredPrimaryWeaponTarget.Name))", Logging.Red);
                                Cache.Instance.PreferredPrimaryWeaponTarget = null;
                            }
                        }

                    }    
                }
                

                foreach (EntityCache KillTargetEntity in Cache.Instance.Targets.Where(e => targetNames.Contains(e.Name) && (e.IsTarget || e.IsTargeting)))
                {
                    if (Cache.Instance.PreferredPrimaryWeaponTarget != null)
                    {
                        if (KillTargetEntity.Id == Cache.Instance.PreferredPrimaryWeaponTarget.Id)
                        {
                            continue;
                        }    
                    }
                    
                    Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Unlocking [" + KillTargetEntity.Name + "][ID: " + Cache.Instance.MaskedID(KillTargetEntity.Id) + "][" + Math.Round(KillTargetEntity.Distance / 1000, 0) + "k away] due to kill order being put on hold", Logging.Teal);
                    KillTargetEntity.UnlockTarget("CombatMissionCtrl");
                }
            }
            else //Do not break aggression on attackers (attack normally)
            {
                //
                // check to see if we have priority targets (ECM, warp scramblers, etc, and let combat process those first)
                //
                EntityCache primaryWeaponPriorityTarget = null;
                try
                {
                    primaryWeaponPriorityTarget = Cache.Instance.PrimaryWeaponPriorityTargets.Where(p => p.Distance < Cache.Instance.MaxRange
                                                                                && p.IsReadyToShoot
                                                                                && p.IsOnGridWithMe
                                                                                && ((!p.IsNPCFrigate && !p.IsFrigate) || (!Cache.Instance.UseDrones && !p.IsTooCloseTooFastTooSmallToHit)))
                                                                               .OrderByDescending(pt => pt.IsTargetedBy)
                                                                               .ThenByDescending(pt => pt.IsInOptimalRange)
                                                                               .ThenByDescending(pt => pt.IsEwarTarget())
                                                                               .ThenBy(pt => pt.PrimaryWeaponPriorityLevel)
                                                                               .ThenBy(pt => pt.Distance)
                                                                               .FirstOrDefault();
                }
                catch (NullReferenceException) { }  // Not sure why this happens, but seems to be no problem

                if (primaryWeaponPriorityTarget != null && primaryWeaponPriorityTarget.IsOnGridWithMe)
                {
                    if (Settings.Instance.DebugKillAction)
                    {
                        if (Cache.Instance.PrimaryWeaponPriorityTargets.Any())
                        {
                            int icount = 0;
                            foreach (EntityCache primaryWeaponPriorityEntity in Cache.Instance.PrimaryWeaponPriorityTargets.Where(i => i.IsOnGridWithMe))
                            {
                                icount++;
                                if (Settings.Instance.DebugKillAction) Logging.Log("Combat", "[" + icount + "] PrimaryWeaponPriorityTarget Named [" + primaryWeaponPriorityEntity.Name + "][ID: " + Cache.Instance.MaskedID(primaryWeaponPriorityEntity.Id) + "][" + Math.Round(primaryWeaponPriorityEntity.Distance / 1000, 0) + "k away]", Logging.Teal);
                                continue;
                            }
                        }
                    }
                    //
                    // getbesttarget below will choose to assign prioritytargets over preferred targets, so we might as well wait... (and not approach the wrong target)
                    //
                }
                else 
                {
                    //
                    // then proceed to kill the target
                    //
                    Cache.Instance.IgnoreTargets.RemoveWhere(targetNames.Contains);

                    EntityCache currentKillTarget = killTargets.OrderBy(t => t.Nearest5kDistance).FirstOrDefault();
                    if (currentKillTarget != null) //if it is not null is HAS to be OnGridWithMe as all killTargets are verified OnGridWithMe
                    {
                        if (Settings.Instance.DebugKillAction) Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], " proceeding to kill the target (this is spammy, but useful debug info)", Logging.White);
                        if (Cache.Instance.PreferredPrimaryWeaponTarget == null || !Cache.Instance.PreferredPrimaryWeaponTarget.IsOnGridWithMe)
                        {
                            Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Adding [" + currentKillTarget.Name + "][" + Math.Round(currentKillTarget.Distance / 1000, 0) + "] groupID [" + currentKillTarget.GroupId + "] TypeID[" + currentKillTarget.TypeId + "] as PreferredPrimaryWeaponTarget", Logging.Teal);
                            Cache.Instance.AddPrimaryWeaponPriorityTargets(killTargets.OrderBy(t => t.Nearest5kDistance).ToList(), PrimaryWeaponPriority.PriorityKillTarget, "CombatMissionCtrl.KillClosestByName");
                            Cache.Instance.PreferredPrimaryWeaponTarget = currentKillTarget;
                        }
                        else if (Settings.Instance.DebugKillAction)
                        {
                            if (Settings.Instance.DebugKillAction) Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Cache.Instance.PreferredPrimaryWeaponTarget =[ " + Cache.Instance.PreferredPrimaryWeaponTarget.Name + " ][" + Cache.Instance.MaskedID(Cache.Instance.PreferredPrimaryWeaponTarget.Id) + "]", Logging.Debug);

                            if (Cache.Instance.PrimaryWeaponPriorityTargets.Any())
                            {
                                if (Settings.Instance.DebugKillAction) Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "PrimaryWeaponPriorityTargets Below (if any)", Logging.Debug);
                                int icount = 0;
                                foreach (EntityCache PT in Cache.Instance.PrimaryWeaponPriorityTargets)
                                {
                                    icount++;
                                    if (Settings.Instance.DebugKillAction) Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "PriorityTarget [" + icount + "] [ " + PT.Name + " ][" + Cache.Instance.MaskedID(PT.Id) + "] IsOnGridWithMe [" + PT.IsOnGridWithMe + "]", Logging.Debug);
                                }
                                if (Settings.Instance.DebugKillAction) Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "PrimaryWeaponPriorityTargets Above (if any)", Logging.Debug);    
                            }
                        }


                        //we may need to get closer so combat will take over
                        if (Cache.Instance.PreferredPrimaryWeaponTarget.Distance > Cache.Instance.MaxRange || Settings.Instance.SpeedTank)
                        {
                            if (Settings.Instance.DebugKillAction) Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "if (Cache.Instance.PreferredPrimaryWeaponTarget.Distance > Cache.Instance.MaxRange)", Logging.Debug);
                            //if (!Cache.Instance.IsApproachingOrOrbiting(Cache.Instance.PreferredPrimaryWeaponTarget.Id))
                            //{
                            //    if (Settings.Instance.DebugKillAction) Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "if (!Cache.Instance.IsApproachingOrOrbiting(Cache.Instance.PreferredPrimaryWeaponTarget.Id))", Logging.Debug);
                                NavigateOnGrid.NavigateIntoRange(Cache.Instance.PreferredPrimaryWeaponTarget, "combatMissionControl", true);
                            //}
                        }
                    }
                }
                
                // GetTargets
                Cache.Instance.GetBestTarget((int)Distances.OnGridWithMe, false, "combat", killTargets.ToList());
                if (Cache.Instance.UseDrones)
                {
                    Cache.Instance.GetBestDroneTarget((int)Distances.OnGridWithMe, false, "Drones", killTargets.ToList());
                }
            }

            // Don't use NextAction here, only if target is killed (checked further up)
            return;
        }

        private void UseDrones(Actions.Action action)
        {
            bool usedrones;
            if (!bool.TryParse(action.GetParameterValue("use"), out usedrones))
            {
                usedrones = true;
            }

            if (!usedrones)
            {
                Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Disable launch of drones", Logging.Teal);
                Cache.Instance.UseDrones = false;
            }
            else
            {
                Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Enable launch of drones", Logging.Teal);
                Cache.Instance.UseDrones = Settings.Instance.UseDrones;
            }
            Nextaction();
            return;
        }

        private void KillClosestByNameAction(Actions.Action action)
        {
            bool notTheClosest;
            if (!bool.TryParse(action.GetParameterValue("notclosest"), out notTheClosest))
            {
                notTheClosest = false;
            }

            if (Cache.Instance.NormalApproach) Cache.Instance.NormalApproach = false;

            List<string> targetNames = action.GetParameterValues("target");

            // No parameter? Ignore kill action
            if (targetNames.Count == 0)
            {
                Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "No targets defined!", Logging.Teal);
                Nextaction();
                return;
            }

            //
            // the way this is currently written is will NOT stop after killing the first target as intended, it will clear all targets with the Name given
            //

            Cache.Instance.AddPrimaryWeaponPriorityTarget(Cache.Instance.potentialCombatTargets.Where(t => targetNames.Contains(t.Name)).OrderBy(t => t.Distance).Take(1).FirstOrDefault(),PrimaryWeaponPriority.PriorityKillTarget, "CombatMissionCtrl.KillClosestByName");
            
            if (Cache.Instance.GetBestTarget((double)Distances.OnGridWithMe, false, "combat", Cache.Instance.potentialCombatTargets.Where(t => targetNames.Contains(t.Name)).OrderBy(t => t.Distance).Take(1).ToList())
                || Cache.Instance.GetBestDroneTarget((double)Distances.OnGridWithMe, false, "Drones", Cache.Instance.potentialCombatTargets.Where(t => targetNames.Contains(t.Name)).OrderBy(t => t.Distance).Take(1).ToList()))
                _clearPocketTimeout = null;

            // Do we have a timeout?  No, set it to now + 5 seconds
            if (!_clearPocketTimeout.HasValue) _clearPocketTimeout = DateTime.UtcNow.AddSeconds(5);

            // Are we in timeout?
            if (DateTime.UtcNow < _clearPocketTimeout.Value) return;

            // We have cleared the Pocket, perform the next action \o/ - reset the timers that we had set for actions...
            Nextaction();

            // Reset timeout
            _clearPocketTimeout = null;
            return;
        }

        private void KillClosestAction(Actions.Action action)
        {
            if (Cache.Instance.NormalApproach) Cache.Instance.NormalApproach = false;

            bool notTheClosest;
            if (!bool.TryParse(action.GetParameterValue("notclosest"), out notTheClosest))
            {
                notTheClosest = false;
            }
            //
            // the way this is currently written is will NOT stop after killing the first target as intended, it will clear all targets with the Name given, in this everything on grid
            //

            if (Cache.Instance.GetBestTarget((double)Distances.OnGridWithMe, false, "combat", Cache.Instance.potentialCombatTargets.Where(e => !e.IsSentry || (e.IsSentry && Settings.Instance.KillSentries)).OrderBy(t => t.Distance).Take(1).ToList())
                || Cache.Instance.GetBestDroneTarget((double)Distances.OnGridWithMe, false, "Drones", Cache.Instance.potentialCombatTargets.Where(e => !e.IsSentry || (e.IsSentry && Settings.Instance.KillSentries)).OrderBy(t => t.Distance).Take(1).ToList()))
                _clearPocketTimeout = null;

            // Do we have a timeout?  No, set it to now + 5 seconds
            if (!_clearPocketTimeout.HasValue) _clearPocketTimeout = DateTime.UtcNow.AddSeconds(5);

            // Are we in timeout?
            if (DateTime.UtcNow < _clearPocketTimeout.Value) return;

            // We have cleared the Pocket, perform the next action \o/ - reset the timers that we had set for actions...
            Nextaction();

            // Reset timeout
            _clearPocketTimeout = null;
            return;
        }

        private void DropItemAction(Actions.Action action)
        {
            Cache.Instance.DropMode = true;
            List<string> items = action.GetParameterValues("item");
            string target = action.GetParameterValue("target");

            int quantity;
            if (!int.TryParse(action.GetParameterValue("quantity"), out quantity))
            {
                quantity = 1;
            }

            bool done = items.Count == 0;

            IEnumerable<EntityCache> targets = Cache.Instance.EntitiesByName(target).ToList();
            if (!targets.Any())
            {
                Logging.Log("MissionController.DropItem", "No target name: " + targets, Logging.Orange);

                // now that we have completed this action revert OpenWrecks to false
                Cache.Instance.DropMode = false;
                openCargoRetryNumber = 0;
                Nextaction();
                return;
            }

            EntityCache closest = targets.OrderBy(t => t.Distance).FirstOrDefault();

            if (closest != null && closest.Distance > (int)Distances.SafeScoopRange)
            {
                if (Cache.Instance.Approaching == null || Cache.Instance.Approaching.Id != closest.Id)
                {
                    if (DateTime.UtcNow > Cache.Instance.NextApproachAction)
                    {
                        Logging.Log("MissionController.DropItem", "Approaching target [" + closest.Name + "][ID: " + Cache.Instance.MaskedID(closest.Id) + "] which is at [" + Math.Round(closest.Distance / 1000, 0) + "k away]", Logging.White);
                        closest.Approach(1000);
                    }
                }
            }
            else
            {
                if (!done)
                {
                    if (DateTime.UtcNow > Cache.Instance.NextOpenContainerInSpaceAction)
                    {
                        if (openCargoRetryNumber > 5)
                        {
                            Logging.Log("CombatMissionCtrl", "Drop: if (openCargoRetryNumber > 5) - error", Logging.Debug);
                            return;
                        }

                        DirectContainer cargo = Cache.Instance.DirectEve.GetShipsCargo();

                        if (closest != null && closest.CargoWindow == null)
                        {
                            openCargoRetryNumber++;
                            Logging.Log("MissionController.DropItem", "Open Cargo", Logging.White);
                            closest.OpenCargo();
                            Cache.Instance.NextOpenContainerInSpaceAction = DateTime.UtcNow.AddSeconds(Cache.Instance.RandomNumber(4, 6));
                            return;
                        }

                        // Get the container that is associated with the cargo container
                        if (closest != null)
                        {
                            DirectContainer container = Cache.Instance.DirectEve.GetContainer(closest.Id);

                            DirectItem itemsToMove = cargo.Items.FirstOrDefault(i => i.TypeName.ToLower() == items.FirstOrDefault().ToLower());
                            if (itemsToMove != null)
                            {
                                Logging.Log("MissionController.DropItem", "Moving Items: " + items.FirstOrDefault() + " from cargo ship to " + container.TypeName, Logging.White);
                                container.Add(itemsToMove, quantity);

                                done = container.Items.Any(i => i.TypeName.ToLower() == items.FirstOrDefault().ToLower() && (i.Quantity >= quantity));
                                Cache.Instance.NextOpenContainerInSpaceAction = DateTime.UtcNow.AddSeconds(Cache.Instance.RandomNumber(4, 6));
                            }
                            else
                            {
                                Logging.Log("MissionController.DropItem", "Error not found Items", Logging.White);
                                Cache.Instance.DropMode = false;
                                Nextaction();
                                return;
                            }
                        }
                    }
                }
                else
                {
                    Logging.Log("MissionController.DropItem", "We are done", Logging.White);

                    // now that we've completed this action revert OpenWrecks to false
                    Cache.Instance.DropMode = false;
                    openCargoRetryNumber = 0;
                    Nextaction();
                    return;
                }
            }

            return;
        }

        private void LootItemAction(Actions.Action action)
        {
            try
            {
                Cache.Instance.CurrentlyShouldBeSalvaging = true;
                Cache.Instance.MissionLoot = true;
                List<string> items = action.GetParameterValues("item");
                List<string> targetNames = action.GetParameterValues("target");

                // if we are not generally looting we need to re-enable the opening of wrecks to
                // find this LootItems we are looking for
                Cache.Instance.OpenWrecks = true;

                int quantity;
                if (!int.TryParse(action.GetParameterValue("quantity"), out quantity))
                {
                    quantity = 1;
                }

                bool done = items.Count == 0;
                if (!done)
                {
                    if (!Cache.Instance.OpenCargoHold("CombatMissionCtrl.LootItemAction")) return;
                    if (Cache.Instance.CargoHold.Window.IsReady)
                    {
                        if (Cache.Instance.CargoHold.Items.Any(i => (items.Contains(i.TypeName) && (i.Quantity >= quantity))))
                        {
                            done = true;
                        }
                    }
                }

                if (done)
                {
                    Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "We are done looting - we have the item(s)", Logging.Teal);

                    // now that we have completed this action revert OpenWrecks to false
                    Cache.Instance.OpenWrecks = false;
                    Cache.Instance.MissionLoot = false;
                    Cache.Instance.CurrentlyShouldBeSalvaging = false;
                    _currentAction++;
                    return;
                }

                IOrderedEnumerable<EntityCache> containers = Cache.Instance.Containers.Where(e => !Cache.Instance.LootedContainers.Contains(e.Id)).OrderBy(e => e.Distance);

                //IOrderedEnumerable<EntityCache> containers = Cache.Instance.Containers.Where(e => !Cache.Instance.LootedContainers.Contains(e.Id)).OrderBy(e => e.Id);
                //IOrderedEnumerable<EntityCache> containers = Cache.Instance.Containers.Where(e => !Cache.Instance.LootedContainers.Contains(e.Id)).OrderByDescending(e => e.Id);
                if (!containers.Any())
                {
                    Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "We are done looting - no containers left to loot", Logging.Teal);
                    Cache.Instance.OpenWrecks = false;
                    Cache.Instance.MissionLoot = false; 
                    Cache.Instance.CurrentlyShouldBeSalvaging = false;
                    _currentAction++;
                    return;
                }

                EntityCache container = containers.FirstOrDefault(c => targetNames.Contains(c.Name)) ?? containers.FirstOrDefault();
                if (container != null && (container.Distance > (int)Distances.SafeScoopRange && (Cache.Instance.Approaching == null || Cache.Instance.Approaching.Id != container.Id)))
                {
                    if (DateTime.UtcNow > Cache.Instance.NextApproachAction && (Cache.Instance.Approaching == null || Cache.Instance.Approaching.Id != container.Id))
                    {
                        Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Approaching target [" + container.Name + "][ID: " + Cache.Instance.MaskedID(container.Id) + "] which is at [" + Math.Round(container.Distance / 1000, 0) + "k away]", Logging.Teal);
                        container.Approach();
                    }
                }
            }
            catch (Exception exception)
            {
                Logging.Log("CombatMissionCtrl.LootItemAction","Exception logged was [" + exception +  "]",Logging.Teal);    
            }

            return;
        }

        //
        // this action still needs some TLC - currently broken (unimplemented)
        //
        /*
                private void SalvageAction(Actions.Action action)
                {
                    Cache.Instance.MissionLoot = true;
                    List<string> items = action.GetParameterValues("item");
                    List<string> targetNames = action.GetParameterValues("target");

                    // if we are not generally looting we need to re-enable the opening of wrecks to
                    // find this LootItems we are looking for
                    Cache.Instance.OpenWrecks = true;

                    //
                    // when the salvage action is 'done' we will be able to open the "target"
                    //
                    bool done = items.Count == 0;
                    if (!done)
                    {
                        DirectContainer cargo = Cache.Instance.DirectEve.GetShipsCargo();

                        // We assume that the ship's cargo will be opened somewhere else
                        if (cargo.Window.IsReady)
                            done |= cargo.Items.Any(i => (items.Contains(i.TypeName)));
                    }
                    if (done)
                    {
                        Logging.Log("CombatMission." + _pocketActions[_currentAction], "We are done looting", Logging.Teal);

                        // now that we have completed this action revert OpenWrecks to false
                        Cache.Instance.OpenWrecks = false;
                        Cache.Instance.MissionLoot = false;
                        _currentAction++;
                        return;
                    }

                    IOrderedEnumerable<EntityCache> containers = Cache.Instance.Containers.Where(e => !Cache.Instance.LootedContainers.Contains(e.Id)).OrderBy(e => e.Distance);
                    if (!containers.Any())
                    {
                        Logging.Log("CombatMission." + _pocketActions[_currentAction], "We are done looting", Logging.Teal);

                        _currentAction++;
                        return;
                    }

                    EntityCache closest = containers.LastOrDefault(c => targetNames.Contains(c.Name)) ?? containers.LastOrDefault();
                    if (closest != null && (closest.Distance > (int)Distance.SafeScoopRange && (Cache.Instance.Approaching == null || Cache.Instance.Approaching.Id != closest.Id)))
                    {
                        if (DateTime.UtcNow > Cache.Instance.NextApproachAction && (Cache.Instance.Approaching == null || Cache.Instance.Approaching.Id != closest.Id))
                        {
                            Logging.Log("CombatMission." + _pocketActions[_currentAction], "Approaching target [" + closest.Name + "][ID: " + closest.Id + "] which is at [" + Math.Round(closest.Distance / 1000, 0) + "k away]", Logging.Teal);
                            closest.Approach();
                        }
                    }
                }
        */

        private void LootAction(Actions.Action action)
        {
            List<string> items = action.GetParameterValues("item");
            List<string> targetNames = action.GetParameterValues("target");

            // if we are not generally looting we need to re-enable the opening of wrecks to
            // find this LootItems we are looking for
            Cache.Instance.OpenWrecks = true;
            Cache.Instance.CurrentlyShouldBeSalvaging = true;

            if (!Settings.Instance.LootEverything)
            {
                bool done = items.Count == 0;
                if (!done)
                {
                    DirectContainer cargo = Cache.Instance.DirectEve.GetShipsCargo();

                    // We assume that the ship's cargo will be opened somewhere else
                    if (cargo.Window.IsReady)
                        done |= cargo.Items.Any(i => items.Contains(i.TypeName));
                }

                if (done)
                {
                    Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "LootEverything:  We are done looting", Logging.Teal);

                    // now that we are done with this action revert OpenWrecks to false
                    Cache.Instance.OpenWrecks = false;
                    Cache.Instance.MissionLoot = false;
                    Cache.Instance.CurrentlyShouldBeSalvaging = false;

                    _currentAction++;
                    return;
                }
            }

            // unlock targets count
            Cache.Instance.MissionLoot = true;

            IOrderedEnumerable<EntityCache> containers = Cache.Instance.Containers.Where(e => !Cache.Instance.LootedContainers.Contains(e.Id)).OrderByDescending(e => e.Id);
            if (!containers.Any())
            {
                // lock targets count
                Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "We are done looting", Logging.Teal);

                // now that we are done with this action revert OpenWrecks to false
                Cache.Instance.OpenWrecks = false;
                Cache.Instance.MissionLoot = false;
                Cache.Instance.CurrentlyShouldBeSalvaging = false;

                _currentAction++;
                return;
            }

            EntityCache container = containers.FirstOrDefault(c => targetNames.Contains(c.Name)) ?? containers.LastOrDefault();
            if (container != null && (container.Distance > (int)Distances.SafeScoopRange && (Cache.Instance.Approaching == null || Cache.Instance.Approaching.Id != container.Id)))
            {
                if (DateTime.UtcNow > Cache.Instance.NextApproachAction)
                {
                    Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Approaching target [" + container.Name + "][ID: " + Cache.Instance.MaskedID(container.Id) + "][" + Math.Round(container.Distance / 1000, 0) + "k away]", Logging.Teal);
                    container.Approach();
                }
            }

            return;
        }

        private void IgnoreAction(Actions.Action action)
        {
            bool clear;
            if (!bool.TryParse(action.GetParameterValue("clear"), out clear))
                clear = false;

            //List<string> removehighestbty = action.GetParameterValues("RemoveHighestBty");
            //List<string> addhighestbty = action.GetParameterValues("AddHighestBty");

            List<string> add = action.GetParameterValues("add");
            List<string> remove = action.GetParameterValues("remove");

            //string targetNames = action.GetParameterValue("target");

            //int distancetoapp;
            //if (!int.TryParse(action.GetParameterValue("distance"), out distancetoapp))
            //    distancetoapp = 1000;

            //IEnumerable<EntityCache> targets = Cache.Instance.Entities.Where(e => targetNames.Contains(e.Name));
            // EntityCache target = targets.OrderBy(t => t.Distance).FirstOrDefault();

            //IEnumerable<EntityCache> targetsinrange = Cache.Instance.Entities.Where(b => Cache.Instance.DistanceFromEntity(b.X ?? 0, b.Y ?? 0, b.Z ?? 0,target) < distancetoapp);
            //IEnumerable<EntityCache> targetsoutofrange = Cache.Instance.Entities.Where(b => Cache.Instance.DistanceFromEntity(b.X ?? 0, b.Y ?? 0, b.Z ?? 0, target) < distancetoapp);

            if (clear)
                Cache.Instance.IgnoreTargets.Clear();
            else
            {
                add.ForEach(a => Cache.Instance.IgnoreTargets.Add(a.Trim()));
                remove.ForEach(a => Cache.Instance.IgnoreTargets.Remove(a.Trim()));
            }
            Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Updated ignore list", Logging.Teal);
            if (Cache.Instance.IgnoreTargets.Any())
                Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Currently ignoring: " + Cache.Instance.IgnoreTargets.Aggregate((current, next) => "[" + current + "][" + next + "]"), Logging.Teal);
            else
                Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Your ignore list is empty", Logging.Teal);
            _currentAction++;
            return;
        }

        private void PerformAction(Actions.Action action)
        {
            switch (action.State)
            {
                case ActionState.LogWhatIsOnGrid:
                    LogWhatIsOnGridAction(action);
                    break;

                case ActionState.Activate:
                    ActivateAction(action);
                    break;

                case ActionState.ClearPocket:
                    ClearPocketAction(action);
                    break;

                case ActionState.ClearAggro:
                    ClearAggroAction(action);
                    break;

                case ActionState.SalvageBookmark:
                    BookmarkPocketForSalvaging();

                    _currentAction++;
                    break;

                case ActionState.Done:
                    DoneAction();
                    break;

                case ActionState.Kill:
                    KillAction(action);
                    break;

                case ActionState.KillOnce:
                    KillAction(action); // TODO Implement
                    break;

                case ActionState.UseDrones:
                    UseDrones(action);
                    break;

                case ActionState.AggroOnly:
                    AggroOnlyAction(action);
                    break;

                case ActionState.KillClosestByName:
                    KillClosestByNameAction(action);
                    break;

                case ActionState.KillClosest:
                    KillClosestAction(action);
                    break;

                case ActionState.MoveTo:
                    MoveToAction(action);
                    break;

                case ActionState.OrbitEntity:
                    OrbitEntityAction(action);
                    break;

                case ActionState.MoveToBackground:
                    MoveToBackgroundAction(action);
                    break;

                case ActionState.ClearWithinWeaponsRangeOnly:
                    ClearWithinWeaponsRangeOnlyAction(action);
                    break;

                case ActionState.ClearWithinWeaponsRangewAggroOnly:
                    ClearWithinWeaponsRangeWithAggroOnlyAction(action);
                    break;

                //case ActionState.Salvage:
                //    SalvageAction(action);
                //    break;

                //case ActionState.Analyze:
                //    AnalyzeAction(action);
                //    break;

                case ActionState.Loot:
                    LootAction(action);
                    break;

                case ActionState.LootItem:
                    LootItemAction(action);
                    break;

                //case ActionState.PutItem:
                //    PutItemAction(action);
                //    break;

                case ActionState.DropItem:
                    DropItemAction(action);
                    break;

                case ActionState.Ignore:
                    IgnoreAction(action);
                    break;

                case ActionState.WaitUntilTargeted:
                    WaitUntilTargeted(action);
                    break;

                case ActionState.DebuggingWait:
                    DebuggingWait(action);
                    break;
            }
        }

        public void ProcessState()
        {
            // There is really no combat in stations (yet)
            if (Cache.Instance.InStation || Settings.Instance.DebugDisableCombatMissionCtrl)
                return;

            // if we are not in space yet, wait...
            if (!Cache.Instance.InSpace)
                return;

            // What? No ship entity?
            if (Cache.Instance.ActiveShip.Entity == null)
                return;

            // There is no combat when cloaked
            if (Cache.Instance.ActiveShip.Entity.IsCloaked)
                return;

            switch (_States.CurrentCombatMissionCtrlState)
            {
                case CombatMissionCtrlState.Idle:
                    break;

                case CombatMissionCtrlState.Done:
                    Statistics.WritePocketStatistics();

                    if (!Cache.Instance.NormalApproach)
                        Cache.Instance.NormalApproach = true;

                    Cache.Instance.IgnoreTargets.Clear();
                    break;

                case CombatMissionCtrlState.Error:
                    break;

                case CombatMissionCtrlState.Start:
                    Cache.Instance.PocketNumber = 0;

                    // Update statistic values
                    Cache.Instance.WealthatStartofPocket = Cache.Instance.DirectEve.Me.Wealth;
                    Statistics.Instance.StartedPocket = DateTime.UtcNow;

                    // Update UseDrones from settings (this can be overridden with a mission action named UseDrones)
                    Cache.Instance.UseDrones = Settings.Instance.UseDrones;

                    // Reload the items needed for this mission from the XML file
                    Cache.Instance.RefreshMissionItems(AgentId);

                    // Reset notNormalNav and onlyKillAggro to false
                    Cache.Instance.normalNav = true;
                    Cache.Instance.onlyKillAggro = false;

                    // Update x/y/z so that NextPocket wont think we are there yet because its checking (very) old x/y/z cords
                    _lastX = Cache.Instance.ActiveShip.Entity.X;
                    _lastY = Cache.Instance.ActiveShip.Entity.Y;
                    _lastZ = Cache.Instance.ActiveShip.Entity.Z;

                    _States.CurrentCombatMissionCtrlState = CombatMissionCtrlState.LoadPocket;
                    break;

                case CombatMissionCtrlState.LoadPocket:
                    _pocketActions.Clear();
                    _pocketActions.AddRange(Cache.Instance.LoadMissionActions(AgentId, Cache.Instance.PocketNumber, true));

                    //
                    // LogStatistics();
                    //
                    if (_pocketActions.Count == 0)
                    {
                        // No Pocket action, load default actions
                        Logging.Log("CombatMissionCtrl", "No mission actions specified, loading default actions", Logging.Orange);

                        // Wait for 30 seconds to be targeted
                        _pocketActions.Add(new Actions.Action { State = ActionState.WaitUntilTargeted });
                        _pocketActions[0].AddParameter("timeout", "15");

                        // Clear the Pocket
                        _pocketActions.Add(new Actions.Action { State = ActionState.ClearPocket });

                        // Is there a gate?
                        if (Cache.Instance.AccelerationGates != null && Cache.Instance.AccelerationGates.Any())
                        {
                            // Activate it (Activate action also moves to the gate)
                            _pocketActions.Add(new Actions.Action { State = ActionState.Activate });
                            _pocketActions[_pocketActions.Count - 1].AddParameter("target", "Acceleration Gate");
                        }
                        else // No, were done
                            _pocketActions.Add(new Actions.Action { State = ActionState.Done });
                    }

                    Logging.Log("-", "-----------------------------------------------------------------", Logging.Teal);
                    Logging.Log("-", "-----------------------------------------------------------------", Logging.Teal);
                    Logging.Log("CombatMissionCtrl", "Mission Timer Currently At: [" + Math.Round(DateTime.UtcNow.Subtract(Statistics.Instance.StartedMission).TotalMinutes, 0) + "] min", Logging.Teal);

                    //if (Cache.Instance.OptimalRange != 0)
                    //    Logging.Log("Optimal Range is set to: " + (Cache.Instance.OrbitDistance / 1000).ToString(CultureInfo.InvariantCulture) + "k");
                    Logging.Log("CombatMissionCtrl", "Max Range is currently: " + (Cache.Instance.MaxRange / 1000).ToString(CultureInfo.InvariantCulture) + "k", Logging.Teal);
                    Logging.Log("-", "-----------------------------------------------------------------", Logging.Teal);
                    Logging.Log("-", "-----------------------------------------------------------------", Logging.Teal);
                    Logging.Log("CombatMissionCtrl", "Pocket [" + Cache.Instance.PocketNumber + "] loaded, executing the following actions", Logging.Orange);
                    int pocketActionCount = 1;
                    foreach (Actions.Action a in _pocketActions)
                    {
                        Logging.Log("CombatMissionCtrl", "Action [ " + pocketActionCount + " ] " + a, Logging.Teal);
                        pocketActionCount++;
                    }
                    Logging.Log("-", "-----------------------------------------------------------------", Logging.Teal);
                    Logging.Log("-", "-----------------------------------------------------------------", Logging.Teal);

                    // Reset pocket information
                    _currentAction = 0;
                    Cache.Instance.IsMissionPocketDone = false;
                    Cache.Instance.IgnoreTargets.Clear();
                    Statistics.PocketObjectStatistics(Cache.Instance.Objects.ToList());
                    _States.CurrentCombatMissionCtrlState = CombatMissionCtrlState.ExecutePocketActions;
                    break;

                case CombatMissionCtrlState.ExecutePocketActions:
                    if (_currentAction >= _pocketActions.Count)
                    {
                        // No more actions, but we're not done?!?!?!
                        Logging.Log("CombatMissionCtrl", "We're out of actions but did not process a 'Done' or 'Activate' action", Logging.Red);

                        _States.CurrentCombatMissionCtrlState = CombatMissionCtrlState.Error;
                        break;
                    }

                    Actions.Action action = _pocketActions[_currentAction];
                    if (action.ToString() != Cache.Instance.CurrentPocketAction)
                    {
                        Cache.Instance.CurrentPocketAction = action.ToString();
                    }
                    int currentAction = _currentAction;
                    PerformAction(action);

                    if (currentAction != _currentAction)
                    {
                        Logging.Log("CombatMissionCtrl", "Finished Action." + action, Logging.Yellow);

                        if (_currentAction < _pocketActions.Count)
                        {
                            action = _pocketActions[_currentAction];
                            Logging.Log("CombatMissionCtrl", "Starting Action." + action, Logging.Yellow);
                        }
                    }

                    if (Settings.Instance.DebugStates)
                        Logging.Log("CombatMissionCtrl", "Action.State = " + action, Logging.Teal);
                    break;

                case CombatMissionCtrlState.NextPocket:
                    double distance = Cache.Instance.DistanceFromMe(_lastX, _lastY, _lastZ);
                    if (distance > (int)Distances.NextPocketDistance)
                    {
                        Logging.Log("CombatMissionCtrl", "We have moved to the next Pocket [" + Math.Round(distance / 1000, 0) + "k away]", Logging.Green);

                        // If we moved more then 100km, assume next Pocket
                        Cache.Instance.ClearEWARCache();
                        Cache.Instance.PocketNumber++;
                        _States.CurrentCombatMissionCtrlState = CombatMissionCtrlState.LoadPocket;
                        Statistics.WritePocketStatistics();
                    }
                    else if (DateTime.UtcNow.Subtract(_moveToNextPocket).TotalMinutes > 2)
                    {
                        Logging.Log("CombatMissionCtrl", "We have timed out, retry last action", Logging.Orange);

                        // We have reached a timeout, revert to ExecutePocketActions (e.g. most likely Activate)
                        _States.CurrentCombatMissionCtrlState = CombatMissionCtrlState.ExecutePocketActions;
                    }
                    break;
            }

            double newX = Cache.Instance.ActiveShip.Entity.X;
            double newY = Cache.Instance.ActiveShip.Entity.Y;
            double newZ = Cache.Instance.ActiveShip.Entity.Z;

            // For some reason x/y/z returned 0 sometimes
            if (newX != 0 && newY != 0 && newZ != 0)
            {
                // Save X/Y/Z so that NextPocket can check if we actually went to the next Pocket :)
                _lastX = newX;
                _lastY = newY;
                _lastZ = newZ;
            }
        }
    }
}