using System;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundTaskComponent
{
    class SunriseSunsetAPI
    {
        public async static Task<RootObject> GetSunTimes(double lat, double lon)
        {
            var http = new HttpClient();
            var url = String.Format("https://api.sunrise-sunset.org/json?lat={0}&lng=-{1}&date=today", lat, lon);
            var response = await http.GetAsync(url);
            var result = await response.Content.ReadAsStringAsync();
            var serializer = new DataContractJsonSerializer(typeof(RootObject));
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(result));
            var data = (RootObject)serializer.ReadObject(ms);
            return data;
        }
    }
    [DataContract]
    public sealed class Results
    {
        [DataMember]
        public string sunrise { get; set; }
        [DataMember]
        public string sunset { get; set; }
        [DataMember]
        public string solar_noon { get; set; }
        [DataMember]
        public string day_length { get; set; }
        [DataMember]
        public string civil_twilight_begin { get; set; }
        [DataMember]
        public string civil_twilight_end { get; set; }
        [DataMember]
        public string nautical_twilight_begin { get; set; }
        [DataMember]
        public string nautical_twilight_end { get; set; }
        [DataMember]
        public string astronomical_twilight_begin { get; set; }
        [DataMember]
        public string astronomical_twilight_end { get; set; }
    }
    [DataContract]
    public sealed class RootObject
    {
        [DataMember]
        public Results results { get; set; }
        [DataMember]
        public string status { get; set; }
    }
}
