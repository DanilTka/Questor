﻿
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using InnerSpaceAPI;

namespace Questor.Modules.Misc
{
    using Questor.Modules.Caching;
    using Questor.Modules.Logging;
    using Questor.Modules.Lookup;
    using Questor.Modules.States;
    using LavishScriptAPI;
    
    public class DoingEventsProperly
    {
        public DoingEventsProperly()
        {
            this.SomethingChanged += HandleSomethingChangedEvent;
        }

        public event EventHandler<PropertyChangedEventArgs> SomethingChanged = (src, ea) => { };

        protected virtual void RaiseSomethingChangedEvent(PropertyChangedEventArgs e) { SomethingChanged(this, e); }
        
        protected virtual void HandleSomethingChangedEvent(object src, PropertyChangedEventArgs ea) { }

    }
    
    public class InnerspaceEvents
    {
        //public InnerspaceCommands() { }

        #region Create Dotnet Events
        //
        // Slave to Master
        //
        private static event EventHandler<LSEventArgs> SomethingChanged_SlaveToMaster_WhatIsCoordofMaster = (src, ea) => { };
        private static uint SlaveToMaster_WhatIsCoordofMaster_InnerspaceEventID;
        private static event EventHandler<LSEventArgs> SomethingChanged_SlaveToMaster_WhatIsCurrentMissionAction = (src, ea) => { };
        private static uint SlaveToMaster_WhatIsCurrentMissionAction_InnerspaceEventID;
        private static event EventHandler<LSEventArgs> SomethingChanged_SlaveToMaster_WhatAmmoShouldILoad = (src, ea) => { };
        private static uint SlaveToMaster_WhatAmmoShouldILoad_InnerspaceEventID;
        private static event EventHandler<LSEventArgs> SomethingChanged_SlaveToMaster_MyListOfPrimaryWeaponPriorityTargets = (src, ea) => { };
        private static uint SlaveToMaster_MyListOfPrimaryWeaponPriorityTargets_InnerspaceEventID;
        //
        // Master to Slave
        //
        private static event EventHandler<LSEventArgs> SomethingChanged_MasterToSlaves_SetDestinationLocationID = (src, ea) => { };
        private static uint MasterToSlaves_SetDestinationLocationID_InnerspaceEventID;
        private static event EventHandler<LSEventArgs> SomethingChanged_MasterToSlaves_MasterCoordinatesAre = (src, ea) => { };
        private static uint MasterToSlaves_MasterCoordinatesAre_InnerspaceEventID;
        private static event EventHandler<LSEventArgs> SomethingChanged_MasterToSlaves_DoThisMissionAction = (src, ea) => { };
        private static uint MasterToSlaves_DoThisMissionAction_InnerspaceEventID;
        private static event EventHandler<LSEventArgs> SomethingChanged_MasterToSlaves_MasterIsWarpingTo = (src, ea) => { };
        private static uint MasterToSlaves_MasterIsWarpingTo_InnerspaceEventID;
        private static event EventHandler<LSEventArgs> SomethingChanged_MasterToSlaves_SlavesGotoBase = (src, ea) => { };
        private static uint MasterToSlaves_SlavesGotoBase_InnerspaceEventID;
        private static event EventHandler<LSEventArgs> SomethingChanged_MasterToSlaves_DoNotLootItemName = (src, ea) => { };
        private static uint MasterToSlaves_DoNotLootItemName_InnerspaceEventID;
        private static event EventHandler<LSEventArgs> SomethingChanged_MasterToSlaves_SetAutoStart = (src, ea) => { };
        private static uint MasterToSlaves_SetAutoStart_InnerspaceEventID;
        private static event EventHandler<LSEventArgs> SomethingChanged_MasterToSlaves_WhereAreYou = (src, ea) => { };
        private static uint MasterToSlaves_WhereAreYou_InnerspaceEventID;
        private static event EventHandler<LSEventArgs> SomethingChanged_MasterToSlaves_WhatAreYouShooting = (src, ea) => { };
        private static uint MasterToSlaves_WhatAreYouShooting_InnerspaceEventID;
        private static event EventHandler<LSEventArgs> SomethingChanged_MasterToSlaves_ShootThisEntityID = (src, ea) => { };
        private static uint MasterToSlaves_ShootThisEntityID_InnerspaceEventID;
        private static event EventHandler<LSEventArgs> SomethingChanged_MasterToSlave_MyListOfPrimaryWeaponPriorityTargets = (src, ea) => { };
        private static uint MasterToSlave_MyListOfPrimaryWeaponPriorityTargets_InnerspaceEventID;
        #endregion Create Dotnet Events

        #region Create Dotnet Event Firing Routines
        internal void Raise_SlaveToMaster_WhatIsCoordofMasterEvent(LSEventArgs _lseventargs)
        {
            SomethingChanged_SlaveToMaster_WhatIsCoordofMaster(this, _lseventargs);
        }

        internal void Raise_SlaveToMaster_WhatIsCurrentMissionAction(LSEventArgs _lseventargs)
        {
            SomethingChanged_SlaveToMaster_WhatIsCurrentMissionAction(this, _lseventargs);
        }

        internal void Raise_SlaveToMaster_WhatAmmoShouldILoad(LSEventArgs _lseventargs)
        {
            SomethingChanged_SlaveToMaster_WhatAmmoShouldILoad(this, _lseventargs);
        }

        internal void Raise_SlaveToMaster_MyListOfPrimaryWeaponPriorityTargets(LSEventArgs _lseventargs)
        {
            SomethingChanged_SlaveToMaster_MyListOfPrimaryWeaponPriorityTargets(this, _lseventargs);
        }

        #endregion Create Dotnet Event Firing Routines

        #region Create Dotnet Event Handling Routines - these are fired BY the event
        //
        // Slave to Master
        //
        internal static void Handle_SlaveToMaster_WhatIsCoordofMasterEvent(object src, LSEventArgs _lseventargs)
        {

        }

        internal static void Handle_SlaveToMaster_WhatIsCurrentMissionAction(object src, LSEventArgs _lseventargs)
        {

        }

        internal static void Handle_SlaveToMaster_WhatAmmoShouldILoad(object src, LSEventArgs _lseventargs)
        {

        }

        internal static void Handle_SlaveToMaster_MyListOfPrimaryWeaponPriorityTargets(object src, LSEventArgs _lseventargs)
        {

        }
        //
        // Master to Slave
        //


        internal static void Handle_MasterToSlaves_SetDestinationLocationID(object src, LSEventArgs _lseventargs)
        {

        }

        internal static void Handle_MasterToSlaves_MasterCoordinatesAre(object src, LSEventArgs _lseventargs)
        {

        }

        internal static void Handle_MasterToSlaves_DoThisMissionAction(object src, LSEventArgs _lseventargs)
        {

        }

        internal static void Handle_MasterToSlaves_MasterIsWarpingTo(object src, LSEventArgs _lseventargs)
        {

        }

        internal static void Handle_MasterToSlaves_SlavesGotoBase(object src, LSEventArgs _lseventargs)
        {

        }

        internal static void Handle_MasterToSlaves_DoNotLootItemName(object src, LSEventArgs _lseventargs)
        {

        }

        internal static void Handle_MasterToSlaves_SetAutoStart(object src, LSEventArgs _lseventargs)
        {

        }

        internal static void Handle_MasterToSlaves_WhereAreYou(object src, LSEventArgs _lseventargs)
        {

        }

        internal static void Handle_MasterToSlaves_WhatAreYouShooting(object src, LSEventArgs _lseventargs)
        {

        }

        internal static void Handle_MasterToSlaves_ShootThisEntityID(object src, LSEventArgs _lseventargs)
        {

        }

        internal static void Handle_MasterToSlave_MyListOfPrimaryWeaponPriorityTargets(object src, LSEventArgs _lseventargs)
        {

        }

        #endregion  Create Dotnet Event Handling Routines - these are fired BY the event

        internal static uint CreateInnerspaceEvent(string _eventNameToRegister, EventHandler<LSEventArgs> _eventhandler)
        {
            uint _innerspaceEventID = LavishScript.Events.RegisterEvent(_eventNameToRegister);
            LavishScript.Events.AttachEventTarget(_eventNameToRegister, _eventhandler);
            return _innerspaceEventID;
        }

        public static void CreateInnerspaceEvents()
        {
            #region When an event fires what will handle it
            SomethingChanged_SlaveToMaster_WhatIsCoordofMaster += Handle_SlaveToMaster_WhatIsCoordofMasterEvent;
            SomethingChanged_SlaveToMaster_WhatIsCurrentMissionAction += Handle_SlaveToMaster_WhatIsCurrentMissionAction;
            SomethingChanged_SlaveToMaster_WhatAmmoShouldILoad += Handle_SlaveToMaster_WhatAmmoShouldILoad;
            SomethingChanged_SlaveToMaster_MyListOfPrimaryWeaponPriorityTargets += Handle_SlaveToMaster_MyListOfPrimaryWeaponPriorityTargets;

            SomethingChanged_MasterToSlaves_SetDestinationLocationID += Handle_MasterToSlaves_SetDestinationLocationID;
            SomethingChanged_MasterToSlaves_MasterCoordinatesAre += Handle_MasterToSlaves_MasterCoordinatesAre;
            SomethingChanged_MasterToSlaves_DoThisMissionAction += Handle_MasterToSlaves_DoThisMissionAction;
            SomethingChanged_MasterToSlaves_MasterIsWarpingTo += Handle_MasterToSlaves_MasterIsWarpingTo;
            SomethingChanged_MasterToSlaves_SlavesGotoBase += Handle_MasterToSlaves_SlavesGotoBase;
            SomethingChanged_MasterToSlaves_DoNotLootItemName += Handle_MasterToSlaves_DoNotLootItemName;
            SomethingChanged_MasterToSlaves_SetAutoStart += Handle_MasterToSlaves_SetAutoStart;
            SomethingChanged_MasterToSlaves_WhereAreYou += Handle_MasterToSlaves_WhereAreYou;
            SomethingChanged_MasterToSlaves_WhatAreYouShooting += Handle_MasterToSlaves_WhatAreYouShooting;
            SomethingChanged_MasterToSlaves_ShootThisEntityID += Handle_MasterToSlaves_ShootThisEntityID;
            SomethingChanged_MasterToSlave_MyListOfPrimaryWeaponPriorityTargets += Handle_MasterToSlave_MyListOfPrimaryWeaponPriorityTargets;
            #endregion When an event fires what will handle it
            
            if (Settings.Instance.UseInnerspace)
            {
                //
                // Slave to Master
                //
                SlaveToMaster_WhatIsCoordofMaster_InnerspaceEventID = CreateInnerspaceEvent("SlaveToMaster_WhatIsCoordofMaster_InnerspaceEvent", SomethingChanged_SlaveToMaster_WhatIsCoordofMaster);
                SlaveToMaster_WhatIsCurrentMissionAction_InnerspaceEventID = CreateInnerspaceEvent("SlaveToMaster_WhatIsCurrentMissionAction_InnerspaceEvent", SomethingChanged_SlaveToMaster_WhatIsCurrentMissionAction);
                SlaveToMaster_WhatAmmoShouldILoad_InnerspaceEventID = CreateInnerspaceEvent("SlaveToMaster_WhatAmmoShouldILoad_InnerspaceEvent", SomethingChanged_SlaveToMaster_WhatAmmoShouldILoad);
                SlaveToMaster_MyListOfPrimaryWeaponPriorityTargets_InnerspaceEventID = CreateInnerspaceEvent("SlaveToMaster_MyListOfPrimaryWeaponPriorityTargets_InnerspaceEvent", SomethingChanged_SlaveToMaster_MyListOfPrimaryWeaponPriorityTargets);
                //
                // Master to Slave
                //
                MasterToSlaves_SetDestinationLocationID_InnerspaceEventID = CreateInnerspaceEvent("MasterToSlaves_SetDestinationLocationID_InnerspaceEvent", SomethingChanged_MasterToSlaves_SetDestinationLocationID);
                MasterToSlaves_MasterCoordinatesAre_InnerspaceEventID = CreateInnerspaceEvent("MasterToSlaves_MasterCoordinatesAre_InnerspaceEvent", SomethingChanged_MasterToSlaves_MasterCoordinatesAre);
                MasterToSlaves_DoThisMissionAction_InnerspaceEventID = CreateInnerspaceEvent("MasterToSlaves_DoThisMissionAction_InnerspaceEvent", SomethingChanged_MasterToSlaves_DoThisMissionAction);
                MasterToSlaves_MasterIsWarpingTo_InnerspaceEventID = CreateInnerspaceEvent("MasterToSlaves_MasterIsWarpingTo_InnerspaceEvent", SomethingChanged_MasterToSlaves_MasterIsWarpingTo);
                MasterToSlaves_SlavesGotoBase_InnerspaceEventID = CreateInnerspaceEvent("MasterToSlaves_SlavesGotoBase_InnerspaceEvent", SomethingChanged_MasterToSlaves_SlavesGotoBase);
                MasterToSlaves_DoNotLootItemName_InnerspaceEventID = CreateInnerspaceEvent("MasterToSlaves_DoNotLootItemName_InnerspaceEvent", SomethingChanged_MasterToSlaves_DoNotLootItemName);
                MasterToSlaves_SetAutoStart_InnerspaceEventID = CreateInnerspaceEvent("MasterToSlaves_SetAutoStart_InnerspaceEvent", SomethingChanged_MasterToSlaves_SetAutoStart);
                MasterToSlaves_WhereAreYou_InnerspaceEventID = CreateInnerspaceEvent("MasterToSlaves_WhereAreYou_InnerspaceEvent", SomethingChanged_MasterToSlaves_WhereAreYou);
                MasterToSlaves_WhatAreYouShooting_InnerspaceEventID = CreateInnerspaceEvent("MasterToSlaves_WhatAreYouShooting_InnerspaceEvent", SomethingChanged_MasterToSlaves_WhatAreYouShooting);
                MasterToSlaves_ShootThisEntityID_InnerspaceEventID = CreateInnerspaceEvent("MasterToSlaves_ShootThisEntityID_InnerspaceEvent", SomethingChanged_MasterToSlaves_ShootThisEntityID);
                MasterToSlave_MyListOfPrimaryWeaponPriorityTargets_InnerspaceEventID = CreateInnerspaceEvent("MasterToSlave_MyListOfPrimaryWeaponPriorityTargets_InnerspaceEvent", SomethingChanged_MasterToSlave_MyListOfPrimaryWeaponPriorityTargets);
                //LavishScript.Events.ExecuteEvent(InnerspaceEventID, "Stuff to communicate here");

                
                //
                // Slaves To Master Requests
                //
                //LavishScript.Events.RegisterEvent()   ("SlaveToMaster_WhatIsLocationIDofMaster", SlaveToMaster_WhatIsLocationIDofMaster_InnerspaceEvent);     //Master should reply: MasterToSlaves_SetDestinationLocationID
                LavishScript.Commands.AddCommand("SlaveToMaster_WhatIsCoordofMaster", QuerySlaveToMaster_WhatIsCoordofMaster);                     //Master should reply: MasterToSlaves_MasterCoordinatesAre_InnerspaceCommand
                LavishScript.Commands.AddCommand("SlaveToMaster_WhatMissionIsCurrentMissionAction", QuerySlaveToMaster_WhatIsCurrentMissionAction);//Master should reply: MasterToSlaves_DoThisMissionAction_InnerspaceCommand
                LavishScript.Commands.AddCommand("SlaveToMaster_WhatAmmoShouldILoad", QuerySlaveToMaster_WhatAmmoShouldILoad);                     //Master should reply: 
                LavishScript.Commands.AddCommand("SlaveToMaster_MyListOfPrimaryWeaponPriorityTargets", QuerySlaveToMaster_MastersListOfPrimaryWeaponPriorityTargets);   //Master should reply: 
                
                //
                // Master To Slaves Requests
                //
                //LavishScript.Commands.AddCommand("MasterToSlaves_SetDestinationLocationID", MasterToSlaves_SetDestinationLocationID_InnerspaceEvent);         //answer to: SlaveToMaster_WhatIsLocationIDofMaster
                //LavishScript.Commands.AddCommand("MasterToSlaves_MasterCoordinatesAre", MasterToSlaves_MasterCoordinatesAre_InnerspaceEvent);                 //answer to: SlaveToMaster_WhatIsCoordofMaster_InnerspaceCommand
                //LavishScript.Commands.AddCommand("MasterToSlaves_DoThisMissionAction", MasterToSlaves_DoThisMissionAction_InnerspaceEvent);                   //answer to: SlaveToMaster_WhatMissionIsCurrentMissionAction
                //LavishScript.Commands.AddCommand("MasterToSlaves_MasterIsWarpingTo", MasterToSlaves_MasterIsWarpingTo_InnerspaceEvent);                       //needs no response
                //LavishScript.Commands.AddCommand("MasterToSlaves_SlavesGotoBase", MasterToSlaves_SlavesGotoBase_InnerspaceEvent);                             //needs no response
                //LavishScript.Commands.AddCommand("MasterToSlaves_DoNotLootItemName", MasterToSlaves_DoNotLootItemName_InnerspaceEvent);                       //needs no response
                //LavishScript.Commands.AddCommand("MasterToSlaves_SetAutoStart", MasterToSlaves_SetAutoStart_InnerspaceEvent);                                 //needs no response
                //LavishScript.Commands.AddCommand("MasterToSlaves_WhereAreYou", MasterToSlaves_WhereAreYou_InnerspaceEvent);                                   //
                //LavishScript.Commands.AddCommand("MasterToSlaves_WhatAreYouShooting", MasterToSlaves_WhatAreYouShooting_InnerspaceEvent);                     //
                //LavishScript.Commands.AddCommand("MasterToSlaves_ShootMyTarget", MasterToSlaves_ShootThisEntityID_InnerspaceEvent);                           //needs no response
                //LavishScript.Commands.AddCommand("MasterToSlave_MyListOfPrimaryWeaponPriorityTargets", MasterToSlave_MyListOfPrimaryWeaponPriorityTargets);   //Master should reply: 

                //LavishScript.Commands.AddCommand("MastersMissionXMLActions", MastersMissionXMLActionsInnerspaceCommand);
                //LavishScript.Commands.AddCommand("RemoteRepairShields", RemoteRepairShieldsInnerspaceCommand);
                //LavishScript.Commands.AddCommand("RemoteArmorRepair", RemoteArmorRepairInnerspaceCommand);

                LavishScript.Commands.AddCommand("ListQuestorEvents", ListQuestorEvents);
            }
        }
        
        #region List Innerspace Commands
        private static int ListQuestorCommands(string[] args)
        {
            Logging.Log("InnerspaceCommands", " ", Logging.White);
            Logging.Log("InnerspaceCommands", " ", Logging.White);
            Logging.Log("InnerspaceCommands", "Questor commands you can run from innerspace", Logging.White);
            Logging.Log("InnerspaceCommands", " ", Logging.White);
            Logging.Log("InnerspaceCommands", "SetAutoStart                                 - SetAutoStart true|false", Logging.White);
            Logging.Log("InnerspaceCommands", "SetDisable3D                                 - SetDisable3D true|false", Logging.White);
            Logging.Log("InnerspaceCommands", "SetExitWhenIdle                              - SetExitWhenIdle true|false", Logging.White);
            Logging.Log("InnerspaceCommands", "SetQuestorStatetoCloseQuestor                - SetQuestorStatetoCloseQuestor true", Logging.White);
            Logging.Log("InnerspaceCommands", "SetQuestorStatetoIdle                        - SetQuestorStatetoIdle true", Logging.White);
            Logging.Log("InnerspaceCommands", "SetCombatMissionsBehaviorStatetoGotoBase     - SetCombatMissionsBehaviorStatetoGotoBase true", Logging.White);
            Logging.Log("InnerspaceCommands", "SetDedicatedBookmarkSalvagerBehaviorStatetoGotoBase     - SetDedicatedBookmarkSalvagerBehaviorStatetoGotoBase true", Logging.White);
            Logging.Log("InnerspaceCommands", " ", Logging.White);
            Logging.Log("InnerspaceCommands", "QuestorCommands                              - (this command) ", Logging.White);
            Logging.Log("InnerspaceCommands", "QuestorEvents                                - Lists the available InnerSpace Events you can listen for ", Logging.White);
            Logging.Log("InnerspaceCommands", " ", Logging.White);
            Logging.Log("InnerspaceCommands", "AddWarpScramblerByName                       - Add NPCs by name to the WarpScramblers List", Logging.White);
            Logging.Log("InnerspaceCommands", "AddWebifierByName                            - Add NPCs by name to the Webifiers List", Logging.White);
            Logging.Log("InnerspaceCommands", "AddIgnoredTarget                             - Add name to the IgnoredTarget List", Logging.White);
            Logging.Log("InnerspaceCommands", "RemoveIgnoredTarget                          - Remove name to the IgnoredTarget List", Logging.White);
            Logging.Log("InnerspaceCommands", "AddDronePriorityTargetsByName                - Add NPCs by name to the DPT List", Logging.White);
            Logging.Log("InnerspaceCommands", "RemoveDronePriorityTargetsByName             - Remove NPCs name from the DPT List", Logging.White);
            Logging.Log("InnerspaceCommands", "AddPrimaryWeaponPriorityTargetsByName        - Add NPCs by name to the PWPT List", Logging.White);
            Logging.Log("InnerspaceCommands", "RemovePrimaryWeaponPriorityTargetsByName     - Remove NPCs name from the PWPT List", Logging.White);
            Logging.Log("InnerspaceCommands", "ListItemHangarItems                          - Logs All Items in the ItemHangar", Logging.White);
            //Logging.Log("InnerspaceCommands", "ListAmmoHangarItems - missing                - Logs All Items in the (optionally configured) AmmoHangar", Logging.White);
            Logging.Log("InnerspaceCommands", "ListLootHangarItems                          - Logs All Items in the (optionally configured) LootHangar", Logging.White);
            Logging.Log("InnerspaceCommands", "ListLootContainerItems                       - Logs All Items in the (optionally configured) LootContainer", Logging.White);
            Logging.Log("InnerspaceCommands", "ListAllEntities                              - Logs All Entities on Grid", Logging.White);
            Logging.Log("InnerspaceCommands", "ListPotentialCombatTargets                   - Logs ListPotentialCombatTargets on Grid", Logging.White);
            Logging.Log("InnerspaceCommands", "ListHighValueTargets                         - Logs ListHighValueTargets on Grid", Logging.White);
            Logging.Log("InnerspaceCommands", "ListLowValueTargets                          - Logs ListLowValueTargets on Grid", Logging.White);
            Logging.Log("InnerspaceCommands", "ListIgnoredTargets                           - Logs the contents of the IgnoredTargets List", Logging.White);
            Logging.Log("InnerspaceCommands", "ListModules                                  - Logs Module List of My Current Ship", Logging.White);
            Logging.Log("InnerspaceCommands", "ListPrimaryWeaponPriorityTargets             - Logs PrimaryWeaponPriorityTargets", Logging.White);
            Logging.Log("InnerspaceCommands", "ListDronePriorityTargets                     - Logs DronePriorityTargets", Logging.White);
            Logging.Log("InnerspaceCommands", "ListTargets                                  - Logs ListTargets", Logging.White);
            Logging.Log("InnerspaceCommands", "ListEntitiesThatHaveUsLocked                 - Logs ListEntitiesThatHaveUsLocked", Logging.White);
            Logging.Log("InnerspaceCommands", "ListClassInstanceInfo                        - Logs Class Instance Info", Logging.White);
            Logging.Log("InnerspaceCommands", "ListCachedPocketInfo                         - Logs Cached Pocket Information", Logging.White);
            //
            // Slaves To Master Communication
            //
            Logging.Log("InnerspaceCommands", "                    Slave To Master Fleet Related Innerspace Commands", Logging.White);
            Logging.Log("InnerspaceCommands", "SlaveToMaster_WhatIsLocationIDofMaster       - Ask Master: What Is the LocationID of the Master (systems and stations are both locationIDs)", Logging.White);
            Logging.Log("InnerspaceCommands", "SlaveToMaster_WhatIsCoordofMaster            - Ask Master: What x,y,z coordinates is the Master at? (assumes you are already in local)", Logging.White);
            Logging.Log("InnerspaceCommands", "SlaveToMaster_WhatIsCurrentMissionAction     - Ask Master: What is the current mission action (if on grid w master)", Logging.White);
            Logging.Log("InnerspaceCommands", "SlaveToMaster_WhatAmmoShouldILoad            - Ask Master: What Ammo DamageType should I load during ARM...", Logging.White);
            //
            // Master To Slaves Communication
            //
            Logging.Log("InnerspaceCommands", "                    Master To Slave Fleet Related Innerspace Commands", Logging.White);
            Logging.Log("InnerspaceCommands", "MasterToSlaves_SetDestinationLocationID      - Tell slaves where to go", Logging.White);
            Logging.Log("InnerspaceCommands", "MasterToSlaves_MasterCoordinatesAre          - Tell slaves where Master is x,y,z coordinates", Logging.White);
            Logging.Log("InnerspaceCommands", "MasterToSlaves_DoThisMissionAction           - Tell slaves to do this mission action", Logging.White);
            Logging.Log("InnerspaceCommands", "MasterToSlaves_MasterIsWarpingTo             - Tell slaves where Master is warping", Logging.White);
            Logging.Log("InnerspaceCommands", "MasterToSlaves_SlavesGotoBase                - Tell slaves to set State to GotoBase", Logging.White);
            Logging.Log("InnerspaceCommands", "MasterToSlaves_DoNotLootItemName             - Tell slaves not to loot this ItemName", Logging.White);
            Logging.Log("InnerspaceCommands", "MasterToSlaves_SetAutoStart                  - Tell slaves to turn autostart on or off", Logging.White);
            Logging.Log("InnerspaceCommands", "MasterToSlaves_WhereAreYou                   - Tell slaves to report locationIDs and coordinates", Logging.White);
            Logging.Log("InnerspaceCommands", "MasterToSlaves_WhatAreYouShooting            - Tell slaves to report what they are shooting currently", Logging.White);
            Logging.Log("InnerspaceCommands", "MasterToSlaves_ShootThisEntityID             - Tell slaves to Add masters Target as Kill Priority Target", Logging.White);
            
            return 0;
        }
        #endregion List Innerspace Commands

        #region Slave to Master Innerspace Commands
        private static int QuerySlaveToMaster_WhatIsCoordofMaster(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceEvents", "SlaveToMaster_WhatIsLocationIDofMaster - What is LocationID of Master", Logging.White);
                return -1;
            }

            Logging.Log("InnerspaceEvents", "Entering InnerspaceEvents.SlaveToMaster_WhatIsLocationIDofMaster", Logging.Debug);
            _States.CurrentInnerspaceEventsState = InnerspaceEventsState.SlaveToMaster_WhatIsLocationIDofMaster;
            return 0;
        }

        private static int QuerySlaveToMaster_WhatIsCurrentMissionAction(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceEvents", "SlaveToMaster_WhatIsCurrentMissionAction - What Mission Action is the Master Running (if any)", Logging.White);
                return -1;
            }

            Logging.Log("InnerspaceEvents", "Entering InnerspaceEvents.SlaveToMaster_WhatIsCurrentMissionAction", Logging.Debug);
            _States.CurrentInnerspaceEventsState = InnerspaceEventsState.SlaveToMaster_WhatIsCurrentMissionAction;
            return 0;
        }

        private static int QuerySlaveToMaster_WhatAmmoShouldILoad(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceEvents", "SlaveToMaster_WhatAmmoShouldILoad - What Ammo should be loaded during Arm?", Logging.White);
                return -1;
            }

            Logging.Log("InnerspaceEvents", "Entering InnerspaceEvents.SlaveToMaster_WhatAmmoShouldILoad", Logging.Debug);
            _States.CurrentInnerspaceEventsState = InnerspaceEventsState.SlaveToMaster_WhatAmmoShouldILoad;
            return 0;
        }

        private static int QuerySlaveToMaster_MastersListOfPrimaryWeaponPriorityTargets(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceEvents", "SlaveToMaster_MastersListOfPrimaryWeaponPriorityTargets - Query the List of Priority Targets from the master", Logging.White);
                return -1;
            }

            Logging.Log("InnerspaceEvents", "Entering InnerspaceEvents.SlaveToMaster_WhatAmmoShouldILoad", Logging.Debug);
            _States.CurrentInnerspaceEventsState = InnerspaceEventsState.SlaveToMaster_MastersListOfPrimaryWeaponPriorityTargets;
            return 0;
        }
        
        #endregion Slave to Master Innerspace Commands

        #region Master To Slave Innerspace Commands
        private static int MasterToSlaves_SetDestinationLocationID_test(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceEvents", "MasterToSlaves_SetDestinationLocationID - Slaves set destination to LocationID (system or station)", Logging.White);
                return -1;
            }

            Logging.Log("InnerspaceEvents", "Entering InnerspaceEvents.MasterToSlaves_SetDestinationLocationID", Logging.Debug);
            _States.CurrentInnerspaceEventsState = InnerspaceEventsState.MasterToSlaves_SetDestinationLocationID;
            return 0;
        }

        private static int MasterToSlaves_MasterCoordinatesAre_test(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceEvents", "MasterToSlaves_MasterCoordinatesAre - Masters x,y,z Coordinates are...", Logging.White);
                return -1;
            }

            Logging.Log("InnerspaceEvents", "Entering InnerspaceEvents.MasterToSlaves_MasterCoordinatesAre", Logging.Debug);
            _States.CurrentInnerspaceEventsState = InnerspaceEventsState.MasterToSlaves_MasterCoordinatesAre;
            return 0;
        }

        private static int MasterToSlaves_MasterIsWarpingTo_test(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceEvents", "MasterToSlaves_MasterIsWarpingTo - Master is warping to EntityName (or bookmark)", Logging.White);
                return -1;
            }

            Logging.Log("InnerspaceEvents", "Entering InnerspaceEvents.MasterToSlaves_MasterIsWarpingTo", Logging.Debug);
            _States.CurrentInnerspaceEventsState = InnerspaceEventsState.MasterToSlaves_MasterIsWarpingTo;
            return 0;
        }

        private static int MasterToSlaves_SlavesGotoBase_test(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceEvents", "MasterToSlaves_SlavesGotoBase - Slaves should set state to GotoBase", Logging.White);
                return -1;
            }

            Logging.Log("InnerspaceEvents", "Entering InnerspaceEvents.MasterToSlaves_SlavesGotoBase", Logging.Debug);
            _States.CurrentInnerspaceEventsState = InnerspaceEventsState.MasterToSlaves_SlavesGotoBase;
            return 0;
        }

        private static int MasterToSlaves_DoThisMissionAction_test(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceEvents", "MasterToSlaves_DoThisMissionAction - Slaves should execute this mission action", Logging.White);
                return -1;
            }

            Logging.Log("InnerspaceEvents", "Entering InnerspaceEvents.MasterToSlaves_DoThisMissionAction", Logging.Debug);
            _States.CurrentInnerspaceEventsState = InnerspaceEventsState.MasterToSlaves_DoThisMissionAction;
            return 0;
        }

        private static int MasterToSlaves_DoNotLootItemName_test(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceEvents", "MasterToSlaves_DoNotLootItemName - Slaves should not loot this ItemName", Logging.White);
                return -1;
            }

            Logging.Log("InnerspaceEvents", "Entering InnerspaceEvents.MasterToSlaves_DoNotLootItemName", Logging.Debug);
            _States.CurrentInnerspaceEventsState = InnerspaceEventsState.MasterToSlaves_DoNotLootItemName;
            return 0;
        }

        private static int MasterToSlaves_SetAutoStart_test(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceEvents", "MasterToSlaves_SetAutoStart - Slaves should Set AutoStart on or off", Logging.White);
                return -1;
            }

            Logging.Log("InnerspaceEvents", "Entering InnerspaceEvents.MasterToSlaves_SetAutoStart", Logging.Debug);
            _States.CurrentInnerspaceEventsState = InnerspaceEventsState.MasterToSlaves_SetAutoStart;
            return 0;
        }

        private static int MasterToSlaves_WhereAreYou_test(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceEvents", "MasterToSlaves_WhereAreYou - Slaves should report where they are located, LocationID and Coordinates", Logging.White);
                return -1;
            }

            Logging.Log("InnerspaceEvents", "Entering InnerspaceEvents.MasterToSlaves_WhereAreYou", Logging.Debug);
            _States.CurrentInnerspaceEventsState = InnerspaceEventsState.MasterToSlaves_WhereAreYou;
            return 0;
        }

        private static int MasterToSlaves_WhatAreYouShooting_test(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceEvents", "MasterToSlaves_WhatAreYouShooting - Slaves should report what they are shooting: Name and EntityID (if any)", Logging.White);
                return -1;
            }

            Logging.Log("InnerspaceEvents", "Entering InnerspaceEvents.MasterToSlaves_WhatAreYouShooting", Logging.Debug);
            _States.CurrentInnerspaceEventsState = InnerspaceEventsState.MasterToSlaves_WhatAreYouShooting;
            return 0;
        }

        private static int MasterToSlaves_ShootThisEntityID_test(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceEvents", "MasterToSlaves_ShootThisEntityID - Slaves should Add this EntityID as a Primary Weapon Priority Kill Target (if any)", Logging.White);
                return -1;
            }

            Logging.Log("InnerspaceEvents", "Entering InnerspaceEvents.MasterToSlaves_ShootThisEntityID", Logging.Debug);
            _States.CurrentInnerspaceEventsState = InnerspaceEventsState.MasterToSlaves_ShootThisEntityID;
            return 0;
        }
        #endregion Master To Slave Innerspace Commands

        #region Slave to Master Routines

        private static bool SlaveToMaster_WhatIsLocationIDofMaster()
        {
            try
            {
                if (Settings.Instance.FleetSupportSlave)
                {
                    //
                    // 
                    //
                    if (DateTime.UtcNow > Cache.Instance.LastSessionChange.AddSeconds(10))
                    {
                        int? _locationID = Cache.Instance.DirectEve.Session.LocationId;
                        if (_locationID != null)
                        {
                            const string RelayToWhere = "all";
                            string LavishCommandToBroadcast = "relay " + RelayToWhere + " " + "-event EVENT_SlaveToMaster_WhatIsLocationIDofMaster";
                            if (Settings.Instance.DebugFleetSupportMaster) InnerSpace.Echo(string.Format("[BroadcastViaInnerspace] " + LavishCommandToBroadcast));
                            LavishScript.ExecuteCommand(LavishCommandToBroadcast);
                            return true;
                        }

                        return false;
                    }

                    return false;
                }

                return true;
            }
            catch (Exception exception)
            {
                Logging.Log("SlaveToMaster_WhatIsLocationIDofMaster", "[" + exception + "]", Logging.Teal);
                return true;
            }
        }

        private static bool SlaveToMaster_WhatIsCoordofMaster()
        {
            try
            {

            }
            catch (Exception exception)
            {
                Logging.Log("SlaveToMaster_WhatIsCoordofMaster", "[" + exception + "]", Logging.Teal);
                return true;
            }

            return false;
        }

        private static bool SlaveToMaster_WhatIsCurrentMissionAction()
        {
            try
            {

            }
            catch (Exception exception)
            {
                Logging.Log("SlaveToMaster_WhatIsCurrentMissionAction", "[" + exception + "]", Logging.Teal);
                return true;
            }

            return false;
        }

        private static bool SlaveToMaster_WhatAmmoShouldILoad()
        {
            try
            {

            }
            catch (Exception exception)
            {
                Logging.Log("SlaveToMaster_WhatAmmoShouldILoad", "[" + exception + "]", Logging.Teal);
                return true;
            }

            return false;
        }
        
        #endregion Slave to Master Routines

        #region Master to Slave Routines

        private static bool MasterToSlaves_SetDestinationLocationID()
        {
            try
            {
                if (DateTime.UtcNow > Cache.Instance.LastSessionChange.AddSeconds(10))
                {
                    int? _locationID = Cache.Instance.DirectEve.Session.LocationId;
                    if (_locationID != null)
                    {
                        const string RelayToWhere = "all";
                        string LavishCommandToBroadcast = "relay " + RelayToWhere + " " + "-event BlahNewEventHere";
                        if (Settings.Instance.DebugFleetSupportMaster) InnerSpace.Echo(string.Format("[BroadcastViaInnerspace] " + LavishCommandToBroadcast));
                        LavishScript.ExecuteCommand(LavishCommandToBroadcast);
                        return true;
                    }

                    return false;
                }
            }
            catch (Exception exception)
            {
                Logging.Log("MasterToSlaves_SetDestinationLocationID", "[" + exception + "]", Logging.Teal);
                return true;
            }

            return false;
        }

        private static bool MasterToSlaves_MasterIsWarpingTo()
        {
            try
            {

            }
            catch (Exception exception)
            {
                Logging.Log("MasterToSlaves_MasterIsWarpingTo", "[" + exception + "]", Logging.Teal);
                return true;
            }

            return false;
        }

        private static bool MasterToSlaves_SlavesGotoBase()
        {
            try
            {

            }
            catch (Exception exception)
            {
                Logging.Log("MasterToSlaves_SlavesGotoBase", "[" + exception + "]", Logging.Teal);
                return true;
            }

            return false;
        }

        private static bool MasterToSlaves_DoThisMissionAction()
        {
            try
            {

            }
            catch (Exception exception)
            {
                Logging.Log("MasterToSlaves_DoThisMissionAction", "[" + exception + "]", Logging.Teal);
                return true;
            }

            return false;
        }

        private static bool MasterToSlaves_DoNotLootItemName()
        {
            try
            {

            }
            catch (Exception exception)
            {
                Logging.Log("MasterToSlaves_DoNotLootItemName", "[" + exception + "]", Logging.Teal);
                return true;
            }

            return false;
        }

        private static bool MasterToSlaves_SetAutoStart()
        {
            try
            {

            }
            catch (Exception exception)
            {
                Logging.Log("MasterToSlaves_SetAutoStart", "[" + exception + "]", Logging.Teal);
                return true;
            }

            return false;
        }

        private static bool MasterToSlaves_WhereAreYou()
        {
            try
            {

            }
            catch (Exception exception)
            {
                Logging.Log("MasterToSlaves_WhereAreYou", "[" + exception + "]", Logging.Teal);
                return true;
            }

            return false;
        }

        private static bool MasterToSlaves_WhatAreYouShooting()
        {
            try
            {

            }
            catch (Exception exception)
            {
                Logging.Log("MasterToSlaves_WhatAreYouShooting", "[" + exception + "]", Logging.Teal);
                return true;
            }

            return false;
        }

        private static bool MasterToSlaves_ShootThisEntityID()
        {
            try
            {

            }
            catch (Exception exception)
            {
                Logging.Log("MasterToSlaves_ShootThisEntityID", "[" + exception + "]", Logging.Teal);
                return true;
            }

            return false;
        }

        #endregion Master to Slave Routines

        private static int ListQuestorEvents(string[] args)
        {
            Logging.Log("InnerspaceEvents", " ", Logging.White);
            Logging.Log("InnerspaceEvents", " ", Logging.White);
            Logging.Log("InnerspaceEvents", "Questor Events you can listen for from an innerspace script", Logging.White);
            Logging.Log("InnerspaceEvents", " ", Logging.White);
            Logging.Log("InnerspaceEvents", "QuestorIdle                                   - This Event fires when entering the QuestorState Idle ", Logging.White);
            Logging.Log("InnerspaceEvents", "QuestorState                                  - This Event fires when the State changes", Logging.White);
            Logging.Log("InnerspaceEvents", "QuestorCombatMissionsBehaviorState            - This Event fires when the State changes", Logging.White);
            Logging.Log("InnerspaceEvents", "QuestorDedicatedBookmarkSalvagerBehaviorState - This Event fires when the State changes", Logging.White);
            Logging.Log("InnerspaceEvents", "QuestorAutoStartState                         - This Event fires when the State changes ", Logging.White);
            Logging.Log("InnerspaceEvents", "QuestorExitWhenIdleState                      - This Event fires when the State changes ", Logging.White);
            Logging.Log("InnerspaceEvents", "QuestorDisable3DState                         - This Event fires when the State changes ", Logging.White);
            Logging.Log("InnerspaceEvents", "QuestorPanicState                             - This Event fires when the State changes ", Logging.White);
            Logging.Log("InnerspaceEvents", "QuestorPausedState                            - This Event fires when the State changes ", Logging.White);
            Logging.Log("InnerspaceEvents", "QuestorDronesState                            - This Event fires when the State changes ", Logging.White);
            Logging.Log("InnerspaceEvents", "QuestorCombatState                            - This Event fires when the State changes ", Logging.White);
            Logging.Log("InnerspaceEvents", "QuestorTravelerState                          - This Event fires when the State changes ", Logging.White);
            Logging.Log("InnerspaceEvents", " ", Logging.White);
            return 0;
        }
        
        public static void LavishEvent_QuestorIdle()
        {
            if (Settings.Instance.UseInnerspace)
            {
                uint QuestorIdleEvent = LavishScript.Events.RegisterEvent("QuestorIdle");
                LavishScript.Events.ExecuteEvent(QuestorIdleEvent, "");
            }
        }

        public static void LavishEvent_QuestorState()
        {
            if (Settings.Instance.UseInnerspace)
            {
                uint QuestorStateEvent = LavishScript.Events.RegisterEvent("QuestorState");
                LavishScript.Events.ExecuteEvent(QuestorStateEvent, _States.CurrentQuestorState.ToString());
            }
        }

        public static void LavishEvent_SkillQueueHasRoom()
        {
            if (Settings.Instance.UseInnerspace)
            {
                uint SkillQueueHasRoomEvent = LavishScript.Events.RegisterEvent("SkillQueueHasRoom");
                LavishScript.Events.ExecuteEvent(SkillQueueHasRoomEvent, "");
            }
        }

        public static void LavishEvent_QuestorCombatMissionsBehaviorState()
        {
            if (Settings.Instance.UseInnerspace)
            {
                uint QuestorCombatMissionsBehaviorStateEvent = LavishScript.Events.RegisterEvent("QuestorCombatMissionsBehaviorState");
                LavishScript.Events.ExecuteEvent(QuestorCombatMissionsBehaviorStateEvent, _States.CurrentCombatMissionBehaviorState.ToString());
            }
        }

        public static void LavishEvent_QuestorDedicatedBookmarkSalvagerBehaviorState()
        {
            if (Settings.Instance.UseInnerspace)
            {
                uint QuestorDedicatedBookmarkSalvagerBehaviorStateEvent = LavishScript.Events.RegisterEvent("QuestorDedicatedBookmarkSalvagerBehaviorState");
                LavishScript.Events.ExecuteEvent(QuestorDedicatedBookmarkSalvagerBehaviorStateEvent, _States.CurrentDedicatedBookmarkSalvagerBehaviorState.ToString());
            }
        }

        public static void LavishEvent_QuestorAutoStartState()
        {
            if (Settings.Instance.UseInnerspace)
            {
                uint QuestorAutoStartStateEvent = LavishScript.Events.RegisterEvent("QuestorAutoStartState");
                LavishScript.Events.ExecuteEvent(QuestorAutoStartStateEvent, Settings.Instance.AutoStart.ToString());
            }
        }

        public static void LavishEvent_QuestorExitWhenIdleState()
        {
            if (Settings.Instance.UseInnerspace)
            {
                uint QuestorExitWhenIdleStateEvent = LavishScript.Events.RegisterEvent("QuestorExitWhenIdleState");
                LavishScript.Events.ExecuteEvent(QuestorExitWhenIdleStateEvent, Cache.Instance.ExitWhenIdle.ToString());
            }
        }

        public static void LavishEvent_QuestorDisable3DState()
        {
            if (Settings.Instance.UseInnerspace)
            {
                uint QuestorDisable3DStateEvent = LavishScript.Events.RegisterEvent("QuestorDisable3DState");
                LavishScript.Events.ExecuteEvent(QuestorDisable3DStateEvent, Settings.Instance.Disable3D.ToString());
            }
        }

        public static void LavishEvent_QuestorPanicState()
        {
            if (Settings.Instance.UseInnerspace)
            {
                uint QuestorPanicStateEvent = LavishScript.Events.RegisterEvent("QuestorPanicState");
                LavishScript.Events.ExecuteEvent(QuestorPanicStateEvent, _States.CurrentPanicState.ToString());
            }
        }

        public static void LavishEvent_QuestorPausedState()
        {
            if (Settings.Instance.UseInnerspace)
            {
                uint QuestorPausedStateEvent = LavishScript.Events.RegisterEvent("QuestorPausedState");
                LavishScript.Events.ExecuteEvent(QuestorPausedStateEvent, Cache.Instance.Paused.ToString());
            }
        }

        public static void LavishEvent_QuestorDronesState()
        {
            if (Settings.Instance.UseInnerspace)
            {
                uint QuestorDronesStateEvent = LavishScript.Events.RegisterEvent("QuestorDronesState");
                LavishScript.Events.ExecuteEvent(QuestorDronesStateEvent, _States.CurrentDroneState.ToString());
            }
        }

        public static void LavishEvent_QuestorCombatState()
        {
            if (Settings.Instance.UseInnerspace)
            {
                uint QuestorCombatStateEvent = LavishScript.Events.RegisterEvent("QuestorCombatState");
                LavishScript.Events.ExecuteEvent(QuestorCombatStateEvent, _States.CurrentCombatState.ToString());
            }
        }

        public static void LavishEvent_QuestorTravelerState()
        {
            if (Settings.Instance.UseInnerspace)
            {
                uint QuestorTravelerStateEvent = LavishScript.Events.RegisterEvent("QuestorTravelerState");
                LavishScript.Events.ExecuteEvent(QuestorTravelerStateEvent, _States.CurrentTravelerState.ToString());
            }
        }

        //public static void LavishEvent_QuestorCurrentSolarSystem()
        //{
        //    if (Settings.Instance.UseInnerspace)
        //    {
        //        uint QuestorCurrentSolarSystemEvent = LavishScript.Events.RegisterEvent("QuestorCurrentSolarSystem");
        //        LavishScript.Events.ExecuteEvent(QuestorCurrentSolarSystemEvent, );
        //    }
        //}

        //public static void LavishEvent_QuestorisInSpace()
        //{
        //    if (Settings.Instance.UseInnerspace)
        //    {
        //        uint QuestorIsInSpaceEvent = LavishScript.Events.RegisterEvent("QuestorIsInSpace");
        //        LavishScript.Events.ExecuteEvent(QuestorIsInSpaceEvent, Cache.Instance.InSpace.ToString());
        //    }
        //}

        public void ProcessState()
        {
            switch (_States.CurrentInnerspaceEventsState)
            {
                case InnerspaceEventsState.Idle:
                    break;
                    
                case InnerspaceEventsState.SlaveToMaster_WhatIsLocationIDofMaster:
                    if (!Cache.Instance.InWarp)
                    {
                        if (Settings.Instance.DebugInnerspaceEvents) Logging.Log("InnerspaceCommands", "InnerspaceEventsState.SlaveToMaster_WhatIsLocationIDofMaster", Logging.Debug);
                        if (!InnerspaceEvents.SlaveToMaster_WhatIsLocationIDofMaster()) return;
                        _States.CurrentInnerspaceEventsState = InnerspaceEventsState.Idle;
                    }
                    break;

                case InnerspaceEventsState.SlaveToMaster_WhatIsCoordofMaster:
                    if (!Cache.Instance.InWarp)
                    {
                        if (Settings.Instance.DebugInnerspaceEvents) Logging.Log("InnerspaceCommands", "InnerspaceEventsState.SlaveToMaster_WhatIsCoordofMaster", Logging.Debug);
                        if (!InnerspaceEvents.SlaveToMaster_WhatIsCoordofMaster()) return;
                        _States.CurrentInnerspaceEventsState = InnerspaceEventsState.Idle;
                        
                    }
                    break;

                case InnerspaceEventsState.SlaveToMaster_WhatIsCurrentMissionAction:
                    if (!Cache.Instance.InWarp)
                    {
                        if (Settings.Instance.DebugInnerspaceEvents) Logging.Log("InnerspaceCommands", "InnerspaceEventsState.SlaveToMaster_WhatIsCurrentMissionAction", Logging.Debug);
                        if (!InnerspaceEvents.SlaveToMaster_WhatIsCurrentMissionAction()) return;
                        _States.CurrentInnerspaceEventsState = InnerspaceEventsState.Idle;
                    }
                    break;

                case InnerspaceEventsState.SlaveToMaster_WhatAmmoShouldILoad:
                    if (!Cache.Instance.InWarp)
                    {
                        if (Settings.Instance.DebugInnerspaceEvents) Logging.Log("InnerspaceCommands", "InnerspaceEventsState.SlaveToMaster_WhatAmmoShouldILoad", Logging.Debug);
                        if (!InnerspaceEvents.SlaveToMaster_WhatAmmoShouldILoad()) return;
                        _States.CurrentInnerspaceEventsState = InnerspaceEventsState.Idle;
                    }
                    break;

                case InnerspaceEventsState.MasterToSlaves_SetDestinationLocationID:
                    if (!Cache.Instance.InWarp)
                    {
                        if (Settings.Instance.DebugInnerspaceEvents) Logging.Log("InnerspaceCommands", "InnerspaceEventsState.MasterToSlaves_SetDestinationLocationID", Logging.Debug);
                        if (!InnerspaceEvents.MasterToSlaves_SetDestinationLocationID()) return;
                        _States.CurrentInnerspaceEventsState = InnerspaceEventsState.Idle;
                    }
                    break;

                case InnerspaceEventsState.MasterToSlaves_MasterIsWarpingTo:
                    if (!Cache.Instance.InWarp)
                    {
                        if (Settings.Instance.DebugInnerspaceEvents) Logging.Log("InnerspaceCommands", "InnerspaceEventsState.MasterToSlaves_MasterIsWarpingTo", Logging.Debug);
                        if (!InnerspaceEvents.MasterToSlaves_MasterIsWarpingTo()) return;
                        _States.CurrentInnerspaceEventsState = InnerspaceEventsState.Idle;
                    }
                    break;

                case InnerspaceEventsState.MasterToSlaves_SlavesGotoBase:
                    if (!Cache.Instance.InWarp)
                    {
                        if (Settings.Instance.DebugInnerspaceEvents) Logging.Log("InnerspaceCommands", "InnerspaceEventsState.MasterToSlaves_SlavesGotoBase", Logging.Debug);
                        if (!InnerspaceEvents.MasterToSlaves_SlavesGotoBase()) return;
                        _States.CurrentInnerspaceEventsState = InnerspaceEventsState.Idle;
                        
                    }
                    break;

                case InnerspaceEventsState.MasterToSlaves_DoThisMissionAction:
                    if (!Cache.Instance.InWarp)
                    {
                        if (Settings.Instance.DebugInnerspaceEvents) Logging.Log("InnerspaceCommands", "InnerspaceEventsState.MasterToSlaves_DoThisMissionAction", Logging.Debug);
                        if (!InnerspaceEvents.MasterToSlaves_DoThisMissionAction()) return;
                        _States.CurrentInnerspaceEventsState = InnerspaceEventsState.Idle;
                        
                    }
                    break;

                case InnerspaceEventsState.MasterToSlaves_DoNotLootItemName:
                    if (!Cache.Instance.InWarp)
                    {
                        if (Settings.Instance.DebugInnerspaceEvents) Logging.Log("InnerspaceCommands", "InnerspaceEventsState.MasterToSlaves_DoNotLootItemName", Logging.Debug);
                        if (!InnerspaceEvents.MasterToSlaves_DoNotLootItemName()) return;
                        _States.CurrentInnerspaceEventsState = InnerspaceEventsState.Idle;
                        
                    }
                    break;

                case InnerspaceEventsState.MasterToSlaves_SetAutoStart:
                    if (!Cache.Instance.InWarp)
                    {
                        if (Settings.Instance.DebugInnerspaceEvents) Logging.Log("InnerspaceCommands", "InnerspaceEventsState.MasterToSlaves_SetAutoStart", Logging.Debug);
                        if (!InnerspaceEvents.MasterToSlaves_SetAutoStart()) return;
                        _States.CurrentInnerspaceEventsState = InnerspaceEventsState.Idle;
                    }
                    break;

                case InnerspaceEventsState.MasterToSlaves_WhereAreYou:
                    if (!Cache.Instance.InWarp)
                    {
                        if (Settings.Instance.DebugInnerspaceEvents) Logging.Log("InnerspaceCommands", "InnerspaceEventsState.MasterToSlaves_WhereAreYou", Logging.Debug);
                        if (!InnerspaceEvents.MasterToSlaves_WhereAreYou()) return;
                        _States.CurrentInnerspaceEventsState = InnerspaceEventsState.Idle;
                    }
                    break;

                case InnerspaceEventsState.MasterToSlaves_WhatAreYouShooting:
                    if (!Cache.Instance.InWarp)
                    {
                        if (Settings.Instance.DebugInnerspaceEvents) Logging.Log("InnerspaceCommands", "InnerspaceEventsState.MasterToSlaves_WhatAreYouShooting", Logging.Debug);
                        if (!InnerspaceEvents.MasterToSlaves_WhatAreYouShooting()) return;
                        _States.CurrentInnerspaceEventsState = InnerspaceEventsState.Idle;
                    }
                    break;

                case InnerspaceEventsState.MasterToSlaves_ShootThisEntityID:
                    if (!Cache.Instance.InWarp)
                    {
                        if (Settings.Instance.DebugInnerspaceEvents) Logging.Log("InnerspaceCommands", "InnerspaceEventsState.MasterToSlaves_ShootThisEntityID", Logging.Debug);
                        if (!InnerspaceEvents.MasterToSlaves_ShootThisEntityID()) return;
                        _States.CurrentInnerspaceEventsState = InnerspaceEventsState.Idle;
                    }
                    break;

                case InnerspaceEventsState.Done:

                    //_lastStatisticsAction = DateTime.UtcNow;
                    _States.CurrentInnerspaceEventsState = InnerspaceEventsState.Idle;
                    break;

                default:

                    // Next state
                    _States.CurrentInnerspaceEventsState = InnerspaceEventsState.Idle;
                    break;
            }
        }
    }
}