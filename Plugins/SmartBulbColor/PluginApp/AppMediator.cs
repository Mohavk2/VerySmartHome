using SmartBulbColor.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommonLibrary;
using System.Windows.Data;
using System.Linq;
using SmartBulbColor.Domain.Entities;
using System.ComponentModel;

namespace SmartBulbColor.PluginApp
{
    sealed class AppMediator : IDisposable
    {
        readonly HsdpDiscoverer Discoverer = new HsdpDiscoverer(
            new HsdpSearchingAtributes
            {
                SsdpMessage = "M-SEARCH * HTTP/1.1\r\n" + "HOST: 239.255.255.250:1982\r\n" + "MAN: \"ssdp:discover\"\r\n" + "ST: wifi_bulb",
                DeviceType = "MiBulbColor",
                MulticastPort = 1982
            });
        readonly BulbRepository Repository = new BulbRepository();
        readonly DtoMapper Mapper = new DtoMapper();
        readonly AmbientLightStreamer AmbientLight = new AmbientLightStreamer();

        public CollectionThreadSafe<ColorBulbProxy> Bulbs { get; } = new CollectionThreadSafe<ColorBulbProxy>();
        public CollectionThreadSafe<ColorBulbProxy> BulbsForAmbientLight { get; private set; } = new CollectionThreadSafe<ColorBulbProxy>();

        public int DeviceCount { get => Repository.Count; }

        public AppMediator()
        {
            Discoverer.ResponsesReceived += OnResponsesReceived;
            Discoverer.StartDiscover();
        }

        public void ToggleAmbientLight(BulbDTO bulbDTO)
        {
            ColorBulbProxy bulb = Repository.GetBulb(bulbDTO.Id);
            if (!BulbsForAmbientLight.Contains(bulb))
            {
                try
                {
                    Discoverer.StopDiscover();
                    BulbsForAmbientLight.Add(bulb);
                    AmbientLight.AddBulbForStreaming(bulb);
                }
                catch (Exception MusicModeFailedException)
                {
                    throw MusicModeFailedException;
                }
            }
            else
            {
                AmbientLight.RemoveBulb(bulb);
                BulbsForAmbientLight.Remove(bulb);
                if (BulbsForAmbientLight.Count == 0)
                    Discoverer.StartDiscover();
            }
        }
        public void TogglePower(BulbDTO bulbDTO)
        {
            ColorBulbProxy bulb = Repository.GetBulb(bulbDTO.Id);
            bulb.PushCommand(BulbCommandBuilder.CreateToggleCommand());
            OnBulbUpdated(bulb);
        }
        public void TurnNormalLightOn(BulbDTO bulbDTO)
        {
            ColorBulbProxy bulb = Repository.GetBulb(bulbDTO.Id);
            bulb.PushCommand(BulbCommandBuilder.CreateSetSceneColorTemperatureCommand(CommandType.RefreshState, 5400, 100));
            OnBulbUpdated(bulb);
        }
        /// <summary>
        /// Sets color mode
        /// </summary>
        /// <param name="value"> 1 - CT mode, 2 - RGB mode , 3 - HSV mode</param>
        public void SetSceneHSV(BulbDTO bulbDTO, HSBColor color)
        {
            ColorBulbProxy bulb = Repository.GetBulb(bulbDTO.Id);
            bulb.PushCommand(BulbCommandBuilder.CreateSetSceneHsvCommand(
                CommandType.Stream, color.Hue, (int)color.Saturation, (int)color.Brightness));
            OnBulbUpdated(bulb);
        }

        /*
         *  Repository
         */

        // Use cases

        public List<BulbDTO> GetBulbs() => Mapper.ToListBulbDTO(Repository.GetAllBulbs());
        public List<GroupDTO> GetGroups() => Mapper.ToListGroupDTO(Repository.GetGroups());
        public List<string> GetGroupNames() => Repository.GetGroupNames();

        public void AddGroup(string groupName)
        {
            Repository.AddGroup(new BulbGroup(groupName));
            OnGroupsUpdated();
        }

        public void RemoveGroup(string groupId)
        {
            Repository.RemoveGroup(groupId);
            OnGroupsUpdated();
        }

        public void AddBulbToGroup(GroupDTO group, BulbDTO bulbDTO)
        {
            BulbGroup updatedGroup = Repository.AddBulbToGroup(group.Id, bulbDTO.Id);
            OnGroupUpdated(updatedGroup);
        }

        public void RemoveBulbFromGroup(GroupDTO group, BulbDTO bulbDTO)
        {
            BulbGroup updatedGroup = Repository.RemoveBulbFromGroup(group.Id, bulbDTO.Id);
            OnGroupUpdated(updatedGroup);
        }

        public void RenameGroup(GroupDTO group, string newGroupName)
        {
            BulbGroup updatedGroup = Repository.RenameGroup(group.Id, newGroupName);
            OnGroupUpdated(updatedGroup);
        }

        //Events

        public delegate void BulbsCollectionEventHandler(List<BulbDTO> allBulbs);
        public event BulbsCollectionEventHandler BulbsCollectionUpdated;
        public delegate void GroupsEventHandler(List<GroupDTO> groups);
        public event GroupsEventHandler GroupsUpdated;
        public delegate void GroupEventHandler(GroupDTO group);
        public event GroupEventHandler GroupUpdated;
        public delegate void BulbEventHandler(BulbDTO bulb);
        public event BulbEventHandler BulbUpdated;

        public void OnBulbsCollectionUpdated(List<ColorBulbProxy> allBulbs)
        {
            BulbsCollectionUpdated?.Invoke(Mapper.ToListBulbDTO(allBulbs));
        }

        public void OnGroupsUpdated()
        {
            GroupsUpdated?.Invoke(Mapper.ToListGroupDTO(Repository.GetGroups()));
        }

        public void OnGroupUpdated(BulbGroup group)
        {
            GroupUpdated?.Invoke(Mapper.ToGroupDTO(group));
        }

        public void OnBulbUpdated(ColorBulbProxy bulb)
        {
            BulbUpdated?.Invoke(Mapper.ToBulbDto(bulb));
        }

        void OnResponsesReceived(List<string> responses)
        {
            List<string> ids = Repository.GetBulbIds();

            if (ids.Count == 0)
            {
                foreach (var response in responses)
                {
                    ColorBulbProxy foundBulb = new ColorBulbProxy(response);
                    Repository.AddBulb(foundBulb);
                }
            }
            else
            {
                foreach (var response in responses)
                {
                    string responseId = ParseBulbIdFromHTTPResponse(response);
                    if (ids.Contains(responseId))
                        continue;
                    else
                    {
                        ColorBulbProxy foundBulb = new ColorBulbProxy(response);
                        Repository.AddBulb(foundBulb);
                    }
                }
            }
            OnBulbsCollectionUpdated(Repository.GetAllBulbs());
        }

        string ParseBulbIdFromHTTPResponse(string response)
        {
            string[] lines = response.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                string[] lineParams = line.Split(new char[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (lineParams.Length == 2)
                {
                    KeyValuePair<string, string> pair = new KeyValuePair<string, string>(lineParams[0].Trim(), lineParams[1].Trim());
                    if (pair.Key == "id")
                        return pair.Value;
                }
            }
            throw new Exception("Cant parse. Response doesn't contains an id!");
        }

        public void Dispose()
        {

        }
    }
}
