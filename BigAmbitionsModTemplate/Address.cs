using Il2CppEnums;

namespace UberSideJobMod
{
    public class Address
    {
        public Il2Cpp.Address gameAddress;
        public string neighborhood { get; set; }
        public string address { get; set; }
        public BusinessTypeName businessType { get; set; }
        public int totalSize { get; set; }
        public string buildingType { get; set; }
        public int customerCapacity { get; set; }
        public int trafficIndex { get; set; }
        public string parkingZone { get; set; }
        public string businessName { get; set; }

        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrEmpty(businessName) && businessName != "Unknown Business")
                    return businessName;
                string type = businessType != BusinessTypeName.Empty
                    ? businessType.ToString()
                    : (!string.IsNullOrEmpty(buildingType) ? buildingType : "Building");
                switch (type.ToLower())
                {
                    case "residential":
                        return $"Apartment";
                    case "warehouse":
                        return $"Warehouse";
                    case "office":
                        return $"Office";
                    case "retail":
                        return $"Shop";
                    case "special":
                        return $"Venue";
                    default:
                        return $"{type}";
                }
            }
        }
    }
}