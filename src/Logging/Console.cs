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

using System;
using System.Collections.Generic;
using ColossalFramework;
using ChirpLogger;

namespace ControlBuildingLevelUpMod {
    public static class Logger {
        const string prefix = "ControlBuildingLevelUpMod: ";

        public static void Info(String message) {
            try {
                DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, prefix + message);
                ChirpLog.Info(prefix + message);
            } catch (Exception e) {
                ChirpLog.Error("Error during Console.Log: " + e);
            }
        }

        public static void Error(String message) {
            try {
                DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Error, prefix + message);
                ChirpLog.Error(prefix + message);
            } catch (Exception e) {
                ChirpLog.Error("Error during Console.Error: " + e);
            }
        }

        public static void Warning(String message) {
            try {
                DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Warning, prefix + message);
                ChirpLog.Warning(prefix + message);
            } catch (Exception e) {
                ChirpLog.Error("Error during Console.Warning: " + e);
            }
        }
    }
}