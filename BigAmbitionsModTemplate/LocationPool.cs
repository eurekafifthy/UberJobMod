using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Il2CppEnums;

namespace UberSideJobMod
{
    public class BussinesTypeNamePool
    {
        public Dictionary<PassengerType, List<BusinessTypeName>> preferredBusinessTypes = new Dictionary<PassengerType, List<BusinessTypeName>>
{
    { PassengerType.Business, new List<BusinessTypeName> {
        BusinessTypeName.Bank,
        BusinessTypeName.LawFirm,
        BusinessTypeName.Headquarters,
        BusinessTypeName.OfficeSupplyStore,
        BusinessTypeName.RecruitmentAgency,
        BusinessTypeName.MarketingAgency,
        BusinessTypeName.WebDevelopmentAgency,
        BusinessTypeName.School,
        BusinessTypeName.Hospital,
        BusinessTypeName.Irs,
        BusinessTypeName.Factory
    } },
    { PassengerType.Tourist, new List<BusinessTypeName> {
        BusinessTypeName.GiftShop,
        BusinessTypeName.Casino,
        BusinessTypeName.Nightclub,
        BusinessTypeName.Florist,
        BusinessTypeName.Bookstore
    } },
    { PassengerType.Party, new List<BusinessTypeName> {
        BusinessTypeName.Nightclub,
        BusinessTypeName.Casino
    } },
    { PassengerType.Regular, new List<BusinessTypeName> {
        BusinessTypeName.CoffeeShop,
        BusinessTypeName.FastFoodRestaurant,
        BusinessTypeName.Supermarket,
        BusinessTypeName.JewelryStore,
        BusinessTypeName.ClothingStore,
        BusinessTypeName.CarDealership,
        BusinessTypeName.ApplianceStore,
        BusinessTypeName.WholesaleStore,
        BusinessTypeName.FurnitureStore,
        BusinessTypeName.LiquorStore,
        BusinessTypeName.ElectronicsStore,
        BusinessTypeName.FruitAndVegetableStore,
        BusinessTypeName.GasStation,
        BusinessTypeName.Gym,
        BusinessTypeName.Hairdresser,
        BusinessTypeName.InteriorInstallationFirm,
        BusinessTypeName.MovingService
    } },
    { PassengerType.Silent, new List<BusinessTypeName>() }
};
    }
}
