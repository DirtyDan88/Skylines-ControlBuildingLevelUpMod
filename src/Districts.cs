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
    class Districts {
        private static Dictionary<ushort, Level[]> districts = new Dictionary<ushort, Level[]>();
        private static System.Object lockDistrict = new System.Object();

        public static void fromByteArray(byte[] data) {
            if (data != null) {
                BinaryFormatter bFormatter = new BinaryFormatter();
                MemoryStream mStream = new MemoryStream(data);
                lock (lockDistrict) {
                    districts = (Dictionary<ushort, Level[]>)bFormatter.Deserialize(mStream);
                }

            } else {
                lock (lockDistrict) {
                    districts = new Dictionary<ushort, Level[]>();
                }
            }
        }

        public static byte[] toByteArray() {
            BinaryFormatter bFormatter = new BinaryFormatter();
            MemoryStream mStream = new MemoryStream();
            bFormatter.Serialize(mStream, districts);

            return mStream.ToArray();
        }

        public static void add(ushort districtID, Level level, int index) {
            Level[] districtLockLevels = getLockLevels(districtID);
            districtLockLevels[index] = level;

            lock (lockDistrict) {
                if (districts.ContainsKey(districtID)) {
                    districts[districtID] = districtLockLevels;
                } else {
                    districts.Add(districtID, districtLockLevels);
                }
            }
        }
        
        /*
        public static void remove(ushort districtID) {
            lock (lockDistrict) {
                districts.Remove(districtID);
            }
        }
        */

        public static void update(ushort districtID, Level level, int index) {
            Level[] districtLockLevels = getLockLevels(districtID);
            districtLockLevels[index] = level;

            lock (lockDistrict) {
                districts[districtID] = districtLockLevels;
            }
        }

        public static Level[] getLockLevels(ushort districtID) {
            Level[] districtLockLevels;

            lock (lockDistrict) {
                if (districts.ContainsKey(districtID)) {
                    districts.TryGetValue(districtID, out districtLockLevels);
                } else {
                    districtLockLevels = new Level[] {Level.None, Level.None, Level.None, Level.None};
                }
            }

            return districtLockLevels;
        }

        public static void dump() {
            int i = 0;
            foreach (KeyValuePair<ushort, Level[]> district in districts) {
                i++;
                Logger.Info("(" + i + ") district " + district.Key + " has lock-levels: " +
                    "\n    Residential: " + district.Value[Buildings.RESIDENTIAL] +
                    "\n    Commercial:  " + district.Value[Buildings.COMMERCIAL] +
                    "\n    Industrial:  " + district.Value[Buildings.INDUSTRIAL] +
                    "\n    Office:      " + district.Value[Buildings.OFFICE]);
            }
        }
    }
}