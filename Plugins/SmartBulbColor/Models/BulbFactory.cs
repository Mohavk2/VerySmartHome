using CommonLibrary;

namespace SmartBulbColor.Models
{
    public class BulbFactory : DeviceFactory
    {
        public override Device CreateDevice(string response)
        {
            return new ColorBulb(response);
        }
    }
}
