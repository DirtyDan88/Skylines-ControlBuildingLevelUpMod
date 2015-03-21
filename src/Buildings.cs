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
using System.Collections.Generic;
using ColossalFramework;
using ColossalFramework.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.IO.Compression;
using System;

namespace ControlBuildingLevelUpMod {
    public static class Buildings {
        /* 
        Since there are no concurrent collections in .NET 3.5 we have to use old-style locking 
        private static ConcurrentDictionary<ushort, Level> buildings = new ConcurrentDictionary<ushort, Level>();
        */
        private static Dictionary<ushort, Level> buildings = new Dictionary<ushort, Level>();
        private static System.Object lockBuilding = new System.Object();

        public static void fromByteArray(byte[] data) {
            if (data != null) {
                BinaryFormatter bFormatter = new BinaryFormatter();
                MemoryStream mStream       = new MemoryStream(data);
                
                buildings = null;
                buildings = (Dictionary<ushort, Level>)bFormatter.Deserialize(mStream);
            }
        }

        public static byte[] toByteArray() {
            BinaryFormatter bFormatter = new BinaryFormatter();
            MemoryStream mStream       = new MemoryStream();
            bFormatter.Serialize(mStream, buildings);

            return mStream.ToArray();
        }

        public static void add(ushort buildingID, Level progressBarLevel) {
            lock (lockBuilding) { 
                buildings.Add(buildingID, progressBarLevel);
            }
        } 

        public static void remove(ushort buildingID) {
            lock (lockBuilding) { 
                buildings.Remove(buildingID);
            }
        }

        public static void update(ushort buildingID, Level progressBarLevel) {
            lock (lockBuilding) { 
                buildings[buildingID] = progressBarLevel;
            }
        }
        
        public static Level getLockLevel(ushort buildingID) {
            Level buildingLockLevel;

            lock (lockBuilding) { 
                if (buildings.ContainsKey(buildingID)) {
                    buildings.TryGetValue(buildingID, out buildingLockLevel);
                } else {
                    buildingLockLevel = Level.None;
                }
            }

            return buildingLockLevel;
        }
        
        public static void dump() {
            int i = 0;
            foreach (KeyValuePair<ushort, Level> building in buildings) {
                i++;
                Logger.Info("(" + i + ") building " + building.Key + " has lock-level " + building.Value);
            }
        }
    }
}