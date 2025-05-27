using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideshareSideJobMod
{
    public class DriverStats
    {
        public float totalEarnings { get; set; } = 0f;
        public int totalRides { get; set; } = 0;
        public float averageRating { get; set; } = 5.0f;
        public List<float> ratings { get; set; } = new List<float>();
        public int cancelledRides { get; set; } = 0;
        public float longestRide { get; set; } = 0f;
        public string mostVisitedNeighborhood { get; set; } = "None";
        public Dictionary<string, int> neighborhoodVisits { get; set; } = new Dictionary<string, int>();
        public float totalTipsEarned { get; set; } = 0f;
        public float highestTip { get; set; } = 0f;
        public int ridesWithTips { get; set; } = 0;
        public float averageTipPercentage { get; set; } = 0f;
        public Dictionary<PassengerType, float> tipsByPassengerType { get; set; } = new Dictionary<PassengerType, float>();
    }
}
