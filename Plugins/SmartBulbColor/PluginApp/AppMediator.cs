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

        public CollectionThreadSafe<ColorBulbProxy> BulbsForAmbientLight { get; private set; } = new CollectionThreadSafe<ColorBulbProxy>();

        public int DeviceCount { get => Repository.Count; }

        public AppMediator()
        {
            Discoverer.ResponsesReceived += OnResponsesReceived;
            Discoverer.StartDiscover();
        }

        public void ToggleAmbientLight(string Id)
        {
            ColorBulbProxy bulb = Repository.GetBulb(Id);
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
        public void TogglePower(string bulbId)
        {
            ColorBulbProxy bulb = Repository.GetBulb(bulbId);
            bulb.PushCommand(BulbCommandBuilder.CreateToggleCommand());
            OnBulbUpdated(Mapper.ToBulbDto(bulb));
        }
        public void TurnNormalLightOn(string bulbId)
        {
            ColorBulbProxy bulb = Repository.GetBulb(bulbId);
            bulb.PushCommand(BulbCommandBuilder.CreateSetSceneColorTemperatureCommand(CommandType.RefreshState, 5400, 100));
            OnBulbUpdated(Mapper.ToBulbDto(bulb));
        }
        /// <summary>
        /// Sets color mode
        /// </summary>
        /// <param name="value"> 1 - CT mode, 2 - RGB mode , 3 - HSV mode</param>
        public void SetSceneHSV(string bulbId, HSBColor color)
        {
            ColorBulbProxy bulb = Repository.GetBulb(bulbId);
            bulb.PushCommand(BulbCommandBuilder.CreateSetSceneHsvCommand(
                CommandType.Stream, color.Hue, (int)color.Saturation, (int)color.Brightness));
            OnBulbUpdated(Mapper.ToBulbDto(bulb));
        }

        public List<BulbDTO> GetBulbs() => Mapper.ToListBulbDTO(Repository.GetAllBulbs());
        public List<GroupDTO> GetGroups() => Mapper.ToListGroupDTO(Repository.GetGroups());
        public List<string> GetGroupNames() => Repository.GetGroupNames();

        public void CreateGroup(string groupName)
        {
            var group = Repository.AddGroup(new BulbGroup(groupName));
            OnGroupCreated(Mapper.ToGroupDTO(group));
        }

        public void DeleteGroup(string groupId)
        {
            var group = Repository.RemoveGroup(groupId);
            OnGroupDeleted(Mapper.ToGroupDTO(group));
        }

        public void RenameGroup(string groupId, string newGroupName)
        {
            BulbGroup updatedGroup = Repository.RenameGroup(groupId, newGroupName);
            OnGroupUpdated(Mapper.ToGroupDTO(updatedGroup));
        }

        public void AddBulbToGroup(string groupId, string bulbId)
        {
            BulbGroup updatedGroup = Repository.AddBulbToGroup(groupId, bulbId);
            OnGroupUpdated(Mapper.ToGroupDTO(updatedGroup));
        }

        public void RemoveBulbFromGroup(string groupId, string bulbId)
        {
            BulbGroup updatedGroup = Repository.RemoveBulbFromGroup(groupId, bulbId);
            OnGroupUpdated(Mapper.ToGroupDTO(updatedGroup));
        }

        //CRUD Events

        public delegate void BulbEventHandler(BulbDTO bulb);
        public event BulbEventHandler BulbCreated;
        public event BulbEventHandler BulbUpdated;
        public event BulbEventHandler BulbDeleted;

        public void OnBulbCreated(BulbDTO bulb) => BulbCreated?.Invoke(bulb);
        public void OnBulbUpdated(BulbDTO bulb) => BulbUpdated?.Invoke(bulb);
        public void OnBulbDeleted(BulbDTO bulb) => BulbDeleted?.Invoke(bulb);

        public delegate void GroupEventHandler(GroupDTO group);
        public event GroupEventHandler GroupCreated;
        public event GroupEventHandler GroupUpdated;
        public event GroupEventHandler GroupDeleted;

        public void OnGroupCreated(GroupDTO group) => GroupCreated?.Invoke(group);
        public void OnGroupUpdated(GroupDTO group) => GroupUpdated?.Invoke(group);
        public void OnGroupDeleted(GroupDTO group) => GroupDeleted?.Invoke(group);

        //On Bulb Found

        void OnResponsesReceived(List<string> responses)
        {
            List<string> ids = Repository.GetBulbIds();

            if (ids.Count == 0)
            {
                foreach (var response in responses)
                {
                    ColorBulbProxy foundBulb = new ColorBulbProxy(response);
                    Repository.AddBulb(foundBulb);
                    OnBulbCreated(Mapper.ToBulbDto(foundBulb));
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
                        OnBulbCreated(Mapper.ToBulbDto(foundBulb));
                    }
                }
            }
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
