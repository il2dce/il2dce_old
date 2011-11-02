// IL2DCE: A dynamic campaign engine for IL-2 Sturmovik: Cliffs of Dover
// Copyright (C) 2011 Stefan Rothdach
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections;
using System.Collections.Generic;

using maddox.game;
using maddox.game.play;
using maddox.game.page;
using maddox.GP;

namespace IL2DCE
{
    public class Core : ICore
    {
        private string careersFolderSystemPath;
        private string campaignsFolderSystemPath;
        private string debugFolderSystemPath;

        public int AdditionalAirOperations
        {
            get
            {
                return this.additionalAirOperations;
            }
        }
        private int additionalAirOperations = 0;

        public int AdditionalGroundOperations
        {
            get
            {
                return this.additionalGroundOperations;
            }
        }
        private int additionalGroundOperations = 0;

        public double FlightSize
        {
            get
            {
                return this.flightSize;
            }
        }
        private double flightSize;

        public double FlightCount
        {
            get
            {
                return this.flightCount;
            }
        }
        private double flightCount;

        public Core(IGame game)
        {
            _gamePlay = game;

            ISectionFile confFile = game.gameInterface.SectionFileLoad("$home/parts/IL2DCE/conf.ini");

            SpawnParked = false;
            if (confFile.exist("Core", "forceSetOnPark"))
            {
                string value = confFile.get("Core", "forceSetOnPark");
                if (value == "1")
                {
                    SpawnParked = true;
                }
                else
                {
                    SpawnParked = false;
                }
            }

            additionalAirOperations = 0;
            if (confFile.exist("Core", "additionalAirOperations"))
            {
                string value = confFile.get("Core", "additionalAirOperations");
                int.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out additionalAirOperations);
            }

            additionalGroundOperations = 0;
            if (confFile.exist("Core", "additionalGroundOperations"))
            {
                string value = confFile.get("Core", "additionalGroundOperations");
                int.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out additionalGroundOperations);
            }

            flightSize = 1.0;
            if (confFile.exist("Core", "flightSize"))
            {
                string value = confFile.get("Core", "flightSize");
                double.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out flightSize);
            }

            flightCount = 1.0;
            if (confFile.exist("Core", "flightCount"))
            {
                string value = confFile.get("Core", "flightCount");
                double.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out flightCount);
            }

            _debug = 0;
            if (confFile.exist("Core", "debug"))
            {
                string value = confFile.get("Core", "debug");
                int.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out _debug);
            }
            
            if (confFile.exist("Main", "campaignsFolder"))
            {
                string campaignsFolderPath = confFile.get("Main", "campaignsFolder");
                this.campaignsFolderSystemPath = game.gameInterface.ToFileSystemPath(campaignsFolderPath);

                System.IO.DirectoryInfo campaignsFolder = new System.IO.DirectoryInfo(campaignsFolderSystemPath);
                if (campaignsFolder.Exists && campaignsFolder.GetDirectories() != null && campaignsFolder.GetDirectories().Length > 0)
                {
                    ISectionFile globalAircraftInfoFile = game.gameInterface.SectionFileLoad(campaignsFolderPath + "/" + "AircraftInfo.ini");
                    foreach (System.IO.DirectoryInfo campaignFolder in campaignsFolder.GetDirectories())
                    {
                        if (campaignFolder.GetFiles("CampaignInfo.ini") != null && campaignFolder.GetFiles("CampaignInfo.ini").Length == 1)
                        {
                            ISectionFile campaignInfoFile = game.gameInterface.SectionFileLoad(campaignsFolderPath + "/" + campaignFolder.Name + "/CampaignInfo.ini");

                            ISectionFile localAircraftInfoFile = null;
                            System.IO.FileInfo localAircraftInfoFileInfo = new System.IO.FileInfo(game.gameInterface.ToFileSystemPath(campaignsFolderPath + "/" + campaignFolder.Name + "/AircraftInfo.ini"));
                            if (localAircraftInfoFileInfo.Exists)
                            {
                                localAircraftInfoFile = game.gameInterface.SectionFileLoad(campaignsFolderPath + "/" + campaignFolder.Name + "/AircraftInfo.ini");
                            }

                            CampaignInfo campaignInfo = new CampaignInfo(campaignFolder.Name, campaignsFolderPath + "/" + campaignFolder.Name + "/", campaignInfoFile, globalAircraftInfoFile, localAircraftInfoFile);
                            CampaignInfos.Add(campaignInfo);
                        }
                    }
                }
            }

            this.careersFolderSystemPath = game.gameInterface.ToFileSystemPath("$user/mission/IL2DCE");
            System.IO.DirectoryInfo careersFolder = new System.IO.DirectoryInfo(careersFolderSystemPath);
            if (careersFolder.Exists && careersFolder.GetDirectories() != null && careersFolder.GetDirectories().Length > 0)
            {
                foreach (System.IO.DirectoryInfo careerFolder in careersFolder.GetDirectories())
                {
                    if (careerFolder.GetFiles("Career.ini") != null && careerFolder.GetFiles("Career.ini").Length == 1)
                    {
                        ISectionFile careerFile = game.gameInterface.SectionFileLoad("$user/mission/IL2DCE" + "/" + careerFolder.Name + "/Career.ini");

                        Career career = new Career(careerFolder.Name, CampaignInfos, careerFile);
                        Careers.Add(career);
                    }
                }
            }

            this.debugFolderSystemPath = game.gameInterface.ToFileSystemPath("$user/missions/IL2DCE/Debug");
        }

        public Core(IGamePlay gamePlay, ISectionFile confFile, string campaignsFolderSystemPath, string careersFolderSystemPath, string debugFolderSystemPath)
        {
            this.campaignsFolderSystemPath = campaignsFolderSystemPath;
            this.careersFolderSystemPath = careersFolderSystemPath;
            this.debugFolderSystemPath = debugFolderSystemPath;

            _gamePlay = gamePlay;

            if (confFile.exist("Core", "forceSetOnPark"))
            {
                string value = confFile.get("Core", "forceSetOnPark");
                if (value == "1")
                {
                    SpawnParked = true;
                }
                else
                {
                    SpawnParked = false;
                }
            }

            if (confFile.exist("Core", "additionalAirOperations"))
            {
                string value = confFile.get("Core", "additionalAirOperations");
                int.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out additionalAirOperations);
            }

            if (confFile.exist("Core", "additionalGroundOperations"))
            {
                string value = confFile.get("Core", "additionalGroundOperations");
                int.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out additionalGroundOperations);
            }

            if (confFile.exist("Core", "debug"))
            {
                string value = confFile.get("Core", "debug");
                int.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out _debug);
            }
            else
            {
                _debug = 0;
            }

            if (confFile.exist("Main", "campaignsFolder"))
            {
                string campaignsFolderPath = confFile.get("Main", "campaignsFolder");
                
                System.IO.DirectoryInfo campaignsFolder = new System.IO.DirectoryInfo(campaignsFolderSystemPath);
                if (campaignsFolder.Exists && campaignsFolder.GetDirectories() != null && campaignsFolder.GetDirectories().Length > 0)
                {
                    ISectionFile globalAircraftInfoFile = GamePlay.gpLoadSectionFile(campaignsFolderPath + "/" + "AircraftInfo.ini");
                    foreach (System.IO.DirectoryInfo campaignFolder in campaignsFolder.GetDirectories())
                    {
                        if (campaignFolder.GetFiles("CampaignInfo.ini") != null && campaignFolder.GetFiles("CampaignInfo.ini").Length == 1)
                        {
                            ISectionFile campaignInfoFile = GamePlay.gpLoadSectionFile(campaignsFolderPath + "/" + campaignFolder.Name + "/CampaignInfo.ini");
                            
                            ISectionFile localAircraftInfoFile = null;
                            System.IO.FileInfo localAircraftInfoFileInfo = new System.IO.FileInfo(campaignsFolderSystemPath  + "\\" + campaignFolder.Name + "\\AircraftInfo.ini");
                            if (localAircraftInfoFileInfo.Exists)
                            {
                                localAircraftInfoFile = GamePlay.gpLoadSectionFile(campaignsFolderPath + "/" + campaignFolder.Name + "/AircraftInfo.ini");
                            }

                            CampaignInfo campaignInfo = new CampaignInfo(campaignFolder.Name, campaignsFolderPath + "/" + campaignFolder.Name + "/", campaignInfoFile, globalAircraftInfoFile, localAircraftInfoFile);
                            CampaignInfos.Add(campaignInfo);
                        }
                    }
                }
            }

            System.IO.DirectoryInfo careersFolder = new System.IO.DirectoryInfo(careersFolderSystemPath);
            if (careersFolder.Exists && careersFolder.GetDirectories() != null && careersFolder.GetDirectories().Length > 0)
            {
                foreach (System.IO.DirectoryInfo careerFolder in careersFolder.GetDirectories())
                {
                    if (careerFolder.GetFiles("Career.ini") != null && careerFolder.GetFiles("Career.ini").Length == 1)
                    {
                        ISectionFile careerFile = GamePlay.gpLoadSectionFile("$user/mission/IL2DCE" + "/" + careerFolder.Name + "/Career.ini");

                        Career career = new Career(careerFolder.Name, CampaignInfos, careerFile);
                        Careers.Add(career);
                    }
                }
            }
        }

        public void DeleteCareer(ICareer career)
        {
            Careers.Remove(career);
            if (CurrentCareer == career)
            {
                CurrentCareer = null;
            }

            List<System.IO.DirectoryInfo> deleteFolders = new List<System.IO.DirectoryInfo>();
            System.IO.DirectoryInfo careersFolder = new System.IO.DirectoryInfo(this.careersFolderSystemPath);
            if (careersFolder.Exists && careersFolder.GetDirectories() != null && careersFolder.GetDirectories().Length > 0)
            {
                foreach (System.IO.DirectoryInfo careerFolder in careersFolder.GetDirectories())
                {
                    if (career.PilotName == careerFolder.Name)
                    {
                        deleteFolders.Add(careerFolder);
                    }
                }
            }

            for (int i = 0; i < deleteFolders.Count; i++)
            {
                deleteFolders[i].Delete(true);
            }
        }

        public ICareer CurrentCareer
        {
            get
            {
                return currentCareer;
            }
            set
            {
                if (currentCareer != value)
                {
                    currentCareer = value;
                }
            }
        }
        private ICareer currentCareer;

        public List<ICareer> Careers
        {
            get
            {
                return _careers;
            }
        }
        private List<ICareer> _careers = new List<ICareer>();

        public List<ICampaignInfo> CampaignInfos
        {
            get
            {
                return campaignInfos;
            }
        }
        private List<ICampaignInfo> campaignInfos = new List<ICampaignInfo>();

        public IGamePlay GamePlay
        {
            get
            {
                return _gamePlay;
            }
        }
        private IGamePlay _gamePlay;

        public IGenerator Generator
        {
            get
            {
                if (this.generator == null)
                {
                    this.generator = new Generator(this);
                }
                return this.generator;
            }
        }
        private IGenerator generator;

        public bool SpawnParked
        {
            get
            {
                return _spawnParked;
            }
            set
            {
                _spawnParked = value;
            }
        }
        public static bool _spawnParked = false;

        public int Debug
        {
            get
            {
                return _debug;
            }
            set
            {
                _debug = value;
            }
        }
        private int _debug = 0;


        public void ResetCampaign(IGame game)
        {
            // Reset campaign state
            CurrentCareer.Date = null;

            AdvanceCampaign(game);
        }

        public void AdvanceCampaign(IGame game)
        {
            if (!CurrentCareer.Date.HasValue)
            {
                CurrentCareer.Date = CurrentCareer.CampaignInfo.StartDate;
                CurrentCareer.Experience = CurrentCareer.RankIndex * 1000;
            }
            else
            {
                CurrentCareer.Date = CurrentCareer.Date.Value.Add(new TimeSpan(1, 0, 0, 0));

                if (game is IGameSingle)
                {
                    if ((game as IGameSingle).BattleSuccess == EBattleResult.SUCCESS)
                    {
                        CurrentCareer.Experience += 200;
                    }
                    else if ((game as IGameSingle).BattleSuccess == EBattleResult.DRAW)
                    {
                        CurrentCareer.Experience += 100;
                    }
                }

                if (CurrentCareer.Experience >= (CurrentCareer.RankIndex + 1) * 1000)
                {
                    CurrentCareer.RankIndex += 1;
                }
            }

            string missionFolderSystemPath = this.careersFolderSystemPath + "\\" + CurrentCareer.PilotName;
            if (!System.IO.Directory.Exists(missionFolderSystemPath))
            {
                System.IO.Directory.CreateDirectory(missionFolderSystemPath);
            }

            string missionId = CurrentCareer.CampaignInfo.Id + "_" + CurrentCareer.Date.Value.Date.Year.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + "-" + CurrentCareer.Date.Value.Date.Month.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + "-" + CurrentCareer.Date.Value.Date.Day.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            ISectionFile missionFile = null;
            IBriefingFile briefingFile = null;
            
            ISectionFile careerFile = GamePlay.gpCreateSectionFile();

            // Preload mission file for path calculation.
            game.gameInterface.MissionLoad(CurrentCareer.CampaignInfo.TemplateFilePath);

            Generate(CurrentCareer.CampaignInfo.TemplateFilePath, missionId, out missionFile, out briefingFile);

            // Stop the preloaded battle to prevent a postload.
            game.gameInterface.BattleStop();

            string missionFileName = string.Format("$user/mission/IL2DCE/" + CurrentCareer.PilotName + "/{0}.mis", missionId);
            string careerFileName = "$user/mission/IL2DCE/" + CurrentCareer.PilotName + "/Career.ini";

            string briefingFileSystemPath = string.Format(this.careersFolderSystemPath + "\\" + CurrentCareer.PilotName + "\\{0}.briefing", missionId);
            string scriptSourceFileSystemPath = this.campaignsFolderSystemPath + "\\" + CurrentCareer.CampaignInfo.Id + "\\" + CurrentCareer.CampaignInfo.ScriptFileName;
            string scriptDestinationFileSystemPath = this.careersFolderSystemPath + "\\" + CurrentCareer.PilotName + "\\" + missionId + ".cs";
            System.IO.File.Copy(scriptSourceFileSystemPath, scriptDestinationFileSystemPath, true);

            missionFile.save(missionFileName);
            briefingFile.save(briefingFileSystemPath);


#if DEBUG
            if (!System.IO.Directory.Exists(this.debugFolderSystemPath))
            {
                System.IO.Directory.CreateDirectory(this.debugFolderSystemPath);
            }
            missionFile.save(  "$user/missions/IL2DCE/Debug/IL2DCEDebug.mis");
            briefingFile.save(this.debugFolderSystemPath + "\\IL2DCEDebug.briefing");
            System.IO.File.Copy(scriptSourceFileSystemPath, this.debugFolderSystemPath + "\\IL2DCEDebug.cs", true);
#else
            if (Debug == 1)
            {
                if (!System.IO.Directory.Exists(this.debugFolderSystemPath))
                {
                    System.IO.Directory.CreateDirectory(this.debugFolderSystemPath);
                }
                missionFile.save(  "$user/missions/IL2DCE/Debug/IL2DCEDebug.mis");
                briefingFile.save(this.debugFolderSystemPath + "\\IL2DCEDebug.briefing");
                System.IO.File.Copy(scriptSourceFileSystemPath, this.debugFolderSystemPath + "\\IL2DCEDebug.cs", true);
            }
#endif

            CurrentCareer.MissionFileName = missionFileName;
            CurrentCareer.writeTo(careerFile);
            careerFile.save(careerFileName);
        }
        
        public void InitCampaign()
        {
            ISectionFile templateFile = this.GamePlay.gpLoadSectionFile(CurrentCareer.CampaignInfo.TemplateFilePath);
            Generator.Init(templateFile);
        }

        public void Generate(string templateFileName, string missionId, out ISectionFile missionFile, out IBriefingFile briefingFile)
        {
            Generator.Generate(templateFileName, missionId, out missionFile, out briefingFile);
        }
    }
}