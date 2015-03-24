/*
    Copyright (c) 2015, Max Stark <max.stark88@web.de> 
        All rights reserved.
    
    This file is part of ControlBuildingLevelUpMod, which is free 
    software: you can redistribute it and/or modify it under the terms 
    of the GNU General Public License as published by the Free 
    Software Foundation, either version 2 of the License, or (at your 
    option) any later version.
    
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
    General Public License for more details. 
    
    You should have received a copy of the GNU General Public License 
    along with this program; if not, see <http://www.gnu.org/licenses/>.
*/

using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using System;
using UnityEngine;

namespace ControlBuildingLevelUpMod {
    public class LevelUpExtension : LevelUpExtensionBase {
        
        public override void OnCreated(ILevelUp levelUp) {
            #if DEBUG
            Logger.Info("LevelUpExtensionBase Created");
            #endif
        }

        public override void OnReleased() {
            #if DEBUG
            Logger.Info("LevelUpExtensionBase Released");
            #endif
        }

        public override ResidentialLevelUp OnCalculateResidentialLevelUp(ResidentialLevelUp levelUp, int averageEducation, int landValue, ushort buildingID, Service service, SubService subService, Level currentLevel) {
            levelUp.targetLevel = this.controlLevelUp(levelUp.targetLevel, currentLevel, buildingID);
            return levelUp;
        }

        public override OfficeLevelUp OnCalculateOfficeLevelUp(OfficeLevelUp levelUp, int averageEducation, int serviceScore, ushort buildingID, Service service, SubService subService, Level currentLevel) {
            levelUp.targetLevel = this.controlLevelUp(levelUp.targetLevel, currentLevel, buildingID);
            return levelUp;
        }
        
        public override CommercialLevelUp OnCalculateCommercialLevelUp(CommercialLevelUp levelUp, int averageWealth, int landValue, ushort buildingID, Service service, SubService subService, Level currentLevel) {
            levelUp.targetLevel = this.controlLevelUp(levelUp.targetLevel, currentLevel, buildingID);
            return levelUp;
        }

        public override IndustrialLevelUp OnCalculateIndustrialLevelUp(IndustrialLevelUp levelUp, int averageEducation, int serviceScore, ushort buildingID, Service service, SubService subService, Level currentLevel) {
            levelUp.targetLevel = this.controlLevelUp(levelUp.targetLevel, currentLevel, buildingID);
            return levelUp;
        }

        private Level determineLockLevel(ushort buildingID) {
            Level buildingLockLevel = Buildings.getLockLevel(buildingID);
            
            if (buildingLockLevel != Level.None) {
                return buildingLockLevel;
            } else {
                Byte districtID = Buildings.getDistrictID(buildingID);
                int buildingType = Buildings.getBuildingType(buildingID);

                Level districtLockLevel = Districts.getLockLevels(districtID)[buildingType];

                if (districtLockLevel != Level.None) {
                    return districtLockLevel;
                } else {
                    return Level.None;
                }
            }
        }

        private Level controlLevelUp(Level targetLevel, Level currentLevel, ushort buildingID) {
            Level lockLevel = this.determineLockLevel(buildingID);

            if (lockLevel != Level.None) {
                #if DEBUG
                Logger.Info("OnCalculateResidentialLevelUp: building: " + buildingID +
                                "\n    lock level is:    " + lockLevel +
                                "\n    current level is: " + currentLevel +
                                "\n    target level is:  " + targetLevel);
                #endif

                if (currentLevel > lockLevel) {
                    #if DEBUG
                    Logger.Info("Building level too high: " + buildingID);
                    #endif
                    
                    this.bulldozeBuilding(buildingID);
                    //TODO: b.m_flags = Building.Flags.Downgrading; ?
                }

                if (targetLevel > lockLevel && targetLevel > currentLevel) {
                    #if DEBUG
                    Logger.Info("Prevent building from leveling up: " + buildingID);
                    #endif

                    return lockLevel;
                }
            }

            return targetLevel;
        }

        private void bulldozeBuilding(ushort buildingID) {
            try {
                BuildingManager buildingManager = Singleton<BuildingManager>.instance;
                Building building = buildingManager.m_buildings.m_buffer[buildingID];
                BuildingInfo buildingInfo = building.Info;

                buildingManager.ReleaseBuilding(buildingID);
                Buildings.remove(buildingID);

                /*
                EffectInfo effect = buildingManager.m_properties.m_bulldozeEffect;
                if (effect != null) {
                    InstanceID instance = new InstanceID();
                    Vector3 pos = building.m_position;
                    float angle = building.m_angle;
                    int length = building.Length;

                    EffectInfo.SpawnArea spawnArea = new EffectInfo.SpawnArea(
                        Matrix4x4.TRS(Building.CalculateMeshPosition(buildingInfo, pos, angle, length),
                        Building.CalculateMeshRotation(angle), Vector3.one), buildingInfo.m_lodMeshData);

                    AudioGroup nullAudioGroup = new AudioGroup(0,
                        new SavedFloat("NOTEXISTINGELEMENT", Settings.gameSettingsFile, 0, false));
                    Singleton<EffectManager>.instance.DispatchEffect(effect, instance, spawnArea,
                        Vector3.zero, 0.0f, 1f, nullAudioGroup);
                }
                */
            } catch (Exception e) {
                Logger.Error("Error during bulldozing building :" + e.Message);
            }
        }
    }
}