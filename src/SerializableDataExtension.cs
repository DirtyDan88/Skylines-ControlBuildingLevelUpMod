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

using ICities;
using System;

namespace ControlBuildingLevelUpMod {
    public class SerializableDataExtension : ISerializableDataExtension {
        private const String DATA_ID = "ControlBuildingLevelUpMod-buildingsLockLevel";
        private ISerializableData sd;

        public void OnCreated(ISerializableData serializedData) {
            #if DEBUG
            Logger.Info("SerializableDataExtension Created");
            #endif

            this.sd = serializedData;
        }

        public void OnReleased() {
            #if DEBUG
            Logger.Info("SerializableDataExtension Released");
            #endif
        }

        public void OnLoadData() {
            #if DEBUG
            Logger.Info("Try to load mod data");
            #endif

            try {
                if (this.sd != null) {
                    byte[] data = this.sd.LoadData(DATA_ID);
                    Buildings.fromByteArray(data);

                    #if DEBUG
                    Logger.Info("Loading was successful");
                    Buildings.dump();
                    #endif

                } else {
                    Logger.Warning("Serializer is null, loading mod data not possible");
                }
                
            } catch (Exception e) {
                Logger.Error("Error during load mod data :" + e.Message);
            }
        }

        public void OnSaveData() {
            #if DEBUG
            Logger.Info("Try to save mod data");
            #endif

            try {
                if (this.sd != null) { 
                    byte[] data = Buildings.toByteArray();
                    if (data != null) {
                        this.sd.SaveData(DATA_ID, data);
                        #if DEBUG
                        Logger.Info("Saving was successful");
                        #endif
                    } 
                } else {
                    Logger.Warning("Serializer is null, saving mod data not possible");
                }
            } catch (Exception e) {
                Logger.Error("Error during save mod data :" + e.Message);
            }
        }
    }
}
