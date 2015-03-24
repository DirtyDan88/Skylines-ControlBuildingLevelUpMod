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
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace ControlBuildingLevelUpMod {
    public class ThreadingExtension : ThreadingExtensionBase {
    
        //private float sumTimeDeltas = 0.0f;
        
        private      UIPanel panelBuildingInfo       = null;
        private      UIPanel panelDistrictInfo       = null;
        private      UIPanel panelBuildingProgress   = null;
        private List<UIPanel> districtProgressPanels = new List<UIPanel>(4);

        public override void OnCreated(IThreading threading) {
            #if DEBUG 
            Logger.Info("ThreadExtensionBase Created");
            #endif
        }

        public override void OnReleased() {
            #if DEBUG 
            Logger.Info("ThreadExtensionBase Released");
            #endif

            if (this.panelBuildingProgress != null) {
                foreach (UIComponent progressBar in this.panelBuildingProgress.components) {
                    UILabel progressBarLabel = progressBar.GetComponentInChildren<UILabel>();
                    progressBar.RemoveUIComponent(progressBarLabel);
                    progressBar.eventClick -= this.BuildingProgressBarClick;
                }
            }

            foreach (UIPanel progressPanel in this.districtProgressPanels) {
                if (progressPanel != null) {
                    foreach (UIComponent progressBar in progressPanel.components) {
                        UILabel progressBarLabel = progressBar.GetComponentInChildren<UILabel>();
                        progressBar.RemoveUIComponent(progressBarLabel);
                        progressBar.eventClick -= this.DistrictProgressBarClick;
                    }
                }
            }

            this.panelBuildingInfo = null;
            this.panelDistrictInfo = null;
        }

        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta) {
            /*
            sumTimeDeltas += realTimeDelta;
            if (sumTimeDeltas > 1.0f) {
                sumTimeDeltas = 0.0f;

            }
            */

            if (this.panelBuildingInfo == null || this.panelBuildingProgress == null) {
                this.initBuildingView();
            } else {
                if (this.panelBuildingInfo.isVisible) {
                    this.updateBuildingView();
                }
            }
            
            bool progressPanelIsNull = false;
            foreach (UIPanel progressPanel in this.districtProgressPanels) {
                if (progressPanel == null) {
                    progressPanelIsNull = true;
                }
            }

            if (this.panelDistrictInfo == null || progressPanelIsNull) {
                this.initDistrictView();
            } else {
                if (this.panelDistrictInfo.isVisible) {
                    this.updateDistrictView();
                }
            }
        }

        private void initBuildingView() {
            this.panelBuildingInfo     = UIView.Find<UIPanel>("(Library) ZonedBuildingWorldInfoPanel");
            this.panelBuildingProgress = UIView.Find<UIPanel>("LevelProgress");

            if (this.panelBuildingProgress != null) {
                foreach (UIComponent progressBar in this.panelBuildingProgress.components) {
                    progressBar.AddUIComponent<UILabel>();
                    progressBar.eventClick += this.BuildingProgressBarClick;
                }
            }
        }

        private void initDistrictView() {
            this.panelDistrictInfo = UIView.Find<UIPanel>("(Library) DistrictWorldInfoPanel");
            this.districtProgressPanels.Add(UIView.Find<UIPanel>("ResidentialLevelProgress"));
            this.districtProgressPanels.Add(UIView.Find<UIPanel>("CommercialLevelProgress"));
            this.districtProgressPanels.Add(UIView.Find<UIPanel>("IndustrialLevelProgress"));
            this.districtProgressPanels.Add(UIView.Find<UIPanel>("OfficeLevelProgress"));

            /*
            this.districtProgressPanels[Districts.RESIDENTIAL] = UIView.Find<UIPanel>("ResidentialLevelProgress");
            this.districtProgressPanels[Districts.COMMERCIAL]  = UIView.Find<UIPanel>("CommercialLevelProgress");
            this.districtProgressPanels[Districts.INDUSTRIAL]  = UIView.Find<UIPanel>("IndustrialLevelProgress");
            this.districtProgressPanels[Districts.OFFICE]      = UIView.Find<UIPanel>("OfficeLevelProgress");
            */

            foreach (UIPanel progressPanel in this.districtProgressPanels) {
                if (progressPanel != null) {
                    foreach (UIComponent progressBar in progressPanel.components) {
                        progressBar.AddUIComponent<UILabel>();
                        progressBar.eventClick += this.DistrictProgressBarClick;
                    }
                }
            }
        }

        private void updateBuildingView() {
            ushort buildingID = this.getSelectedBuildingID();
            Level buildingLockLevel = Buildings.getLockLevel(buildingID);

            this.updateBuildingView(buildingID, buildingLockLevel);
        }

        private void updateBuildingView(ushort buildingID, Level buildingLockLevel) {
            if (buildingLockLevel != Level.None) {
                this.updateProgressPanel(this.panelBuildingProgress, buildingLockLevel, false);

            } else {
                Byte districtID  = Buildings.getDistrictID(buildingID);
                int buildingType = Buildings.getBuildingType(buildingID);
                Level districtLockLevel = Districts.getLockLevels(districtID)[buildingType];

                if (districtLockLevel != Level.None) {
                    this.updateProgressPanel(this.panelBuildingProgress, districtLockLevel, true);
                } else {
                    this.updateProgressPanel(this.panelBuildingProgress, Level.None, false);
                }
            }
        }

        private void updateDistrictView() {
            Level[] districtLockLevels = Districts.getLockLevels(this.getSelectedDistrictID());
            
            int index = 0;
            foreach (UIPanel progressPanel in this.districtProgressPanels) {
                this.updateProgressPanel(progressPanel, districtLockLevels[index], true);
                index++;
            }
        }

        private void updateProgressPanel(UIComponent progressPanel, Level lockLevel, bool colored) {
            foreach (UIComponent progressBar in progressPanel.components) {
                Level progressBarLevel = this.getProgressBarLevel(progressBar);
                UILabel progressBarLabel = progressBar.GetComponentInChildren<UILabel>();

                float x = progressBar.width / 2 - progressBarLabel.width / 2;
                float y = progressBar.height / 2 - progressBarLabel.height / 2;
                progressBarLabel.relativePosition = new Vector3(x, y);

                progressBarLabel.text = "";
                if (lockLevel == progressBarLevel) {
                    if (colored) {
                        //progressBarLabel.textColor = new Color32(255, 255, 0, 255);
                        progressBarLabel.textColor = new Color32(255, 0, 0, 255);
                    } else {
                        progressBarLabel.textColor = new Color32(255, 255, 255, 255);
                    }
                    progressBarLabel.text = "x"; //"#"; //"■";
                }
            }
        }

        private void BuildingProgressBarClick(UIComponent progressBar, UIMouseEventParameter eventParam) {
            ushort buildingID = this.getSelectedBuildingID();
            Level buildingLockLevel = Buildings.getLockLevel(buildingID);
            Level progressBarLevel  = this.getProgressBarLevel(progressBar);

            if (buildingLockLevel == Level.None) {
                Buildings.add(buildingID, progressBarLevel);
                this.updateBuildingView(buildingID, progressBarLevel);
                #if DEBUG 
                Logger.Info("building lock level (" + buildingID + "): " + progressBarLevel);
                #endif

            } else {
                if (buildingLockLevel == progressBarLevel) {
                    Buildings.remove(buildingID);
                    this.updateBuildingView(buildingID, Level.None);
                    #if DEBUG 
                    Logger.Info("building lock level (" + buildingID + "): none");
                    #endif

                } else {
                    Buildings.update(buildingID, progressBarLevel);
                    this.updateBuildingView(buildingID, progressBarLevel);
                    #if DEBUG
                    Logger.Info("building lock level (" + buildingID + "): " + progressBarLevel);
                    #endif
                }
            }
        }

        private void DistrictProgressBarClick(UIComponent progressBar, UIMouseEventParameter eventParam) {
            ushort districtID = this.getSelectedDistrictID();
            int districtType = this.getProgressBarType(progressBar.parent);
            UIComponent progressPanel = progressBar.parent;
            Level[] districtLockLevels = Districts.getLockLevels(districtID);
            Level progressBarLevel = this.getProgressBarLevel(progressBar);

            if (districtLockLevels[districtType] == Level.None) {
                Districts.add(districtID, progressBarLevel, districtType);
                this.updateProgressPanel(progressPanel, progressBarLevel, true);
                #if DEBUG
                Logger.Info("district lock level (" + districtType + " | " + districtID + "): " + progressBarLevel);
                #endif

            } else {
                if (districtLockLevels[districtType] == progressBarLevel) {
                    Districts.update(districtID, Level.None, districtType);
                    this.updateProgressPanel(progressPanel, Level.None, true);
                    #if DEBUG
                    Logger.Info("district lock level (" + districtType + " | " + districtID + "): " + Level.None);
                    #endif

                } else {
                    Districts.update(districtID, progressBarLevel, districtType);
                    this.updateProgressPanel(progressPanel, progressBarLevel, true);
                    #if DEBUG
                    Logger.Info("district lock level (" + districtType + " | " + districtID + "): " + progressBarLevel);
                    #endif
                }
            }
        }

        private Level getProgressBarLevel(UIComponent progressBar) {
            switch (progressBar.name) {
                case "Level1Bar": return Level.Level1;
                case "Level2Bar": return Level.Level2;
                case "Level3Bar": return Level.Level3;
                case "Level4Bar": return Level.Level4;
                case "Level5Bar": return Level.Level5;
                default: return Level.None;
            }
        }

        private int getProgressBarType(UIComponent progressPanel) {
            switch (progressPanel.name) {
                case "ResidentialLevelProgress": return Buildings.RESIDENTIAL;
                case "CommercialLevelProgress":  return Buildings.COMMERCIAL;
                case "IndustrialLevelProgress":  return Buildings.INDUSTRIAL;
                case "OfficeLevelProgress":      return Buildings.OFFICE;
                default: return -1;
            }
        }

        private ushort getSelectedBuildingID() {
            ZonedBuildingWorldInfoPanel panel = this.panelBuildingInfo.gameObject.
                                                GetComponent<ZonedBuildingWorldInfoPanel>();
            return Convert.ToUInt16(
                    this.getProperty("Index", this.getField("m_InstanceID", panel))
                    .ToString());
        }

        private ushort getSelectedDistrictID() {
            DistrictWorldInfoPanel panel = this.panelDistrictInfo.gameObject.
                                                GetComponent<DistrictWorldInfoPanel>();
            return Convert.ToUInt16(
                    this.getProperty("Index", this.getField("m_InstanceID", panel))
                    .ToString());
        }

        private System.Object getField(String name, System.Object obj) {
            MemberInfo[] members = obj.GetType().GetMembers(BindingFlags.Instance |
                                                            BindingFlags.NonPublic |
                                                            BindingFlags.Public |
                                                            BindingFlags.Static);
            foreach (MemberInfo member in members) {
                if (member.MemberType == MemberTypes.Field) {
                    FieldInfo field = (FieldInfo)member;
                    if (field.Name.Equals(name)) {
                        return field.GetValue(obj);
                    }
                }
            }

            return null;
        }

        private System.Object getProperty(String name, System.Object obj) {
            MemberInfo[] members = obj.GetType().GetMembers(BindingFlags.Instance |
                                                            BindingFlags.NonPublic |
                                                            BindingFlags.Public |
                                                            BindingFlags.Static);
            foreach (MemberInfo member in members) {
                if (member.MemberType == MemberTypes.Property) {
                    PropertyInfo property = (PropertyInfo)member;
                    if (property.Name.Equals(name)) {
                        return property.GetValue(obj, null);
                    }
                }
            }

            return null;
        }
    }
}