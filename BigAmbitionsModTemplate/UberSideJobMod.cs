using MelonLoader;
using MelonLoader.Utils;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Il2Cpp;
using Il2CppUI.PlayerHUD;
using Il2CppBuildings.Outdoors;
using Il2CppCharacter;
using Il2CppBigAmbitions.Characters;
using Il2CppUI.Smartphone.Apps.Persona;
using Il2CppVehicles.VehicleTypes;
using Il2CppEnums;
using Newtonsoft.Json;
using UberSideJobMod.UberSideJobMod;
using UnityEngine.EventSystems;

namespace UberSideJobMod
{
    public class UberJobMod : MelonMod
    {
        #region Fields
        // Queue system 
        private Queue<UberPassenger> passengerPool;
        private const float MaxPoolTime = 60f;

        // Constants
        private float sessionStartTime = 0f;
        private float lastCollisionDebounceTime = 0f;
        private float speedingTimer = 0f;
        private bool wasSpeeding = false;
        private const float CheckDistance = 30f;
        private const float PickupConfirmTime = 0.15f;
        private float timeSinceLastOutsideZone = 0f;
        private const float PassengerOfferInterval = 30f;
        private const float UberJobCooldown = 120f;
        private const float MaxWaitingTime = 300f;
        private const float ConversationInterval = 20f;
        private const float CollisionRecoveryTime = 8f;
        private const float AccelerationThreshold = 10f;
        private const float SmoothAccelerationThreshold = 15f;
        private const float PeakMultiplier = 1.5f;
        private const float SuddenStopCooldownDuration = 10f;
        private bool isCompletingJob = false;

        // Tips System
        private readonly float baselineTipRate = 0.15f;
        private readonly float tipMultiplierForGoodDriving = 1.5f;
        private readonly float tipMultiplierForBadDriving = 0.5f;
        private readonly Dictionary<PassengerType, float> passengerTypeTipRates = new Dictionary<PassengerType, float>();
        private readonly Dictionary<string, float> neighborhoodTipRates = new Dictionary<string, float>();

        // Core State
        private bool uberJobActive = false;
        private bool showUberUI = false;
        private UberPassenger currentPassenger = null;
        private DriverStats driverStats = new DriverStats();
        private List<Address> addresses = new List<Address>();
        private UberUIManager uiManager;
        private UberNotificationUI notificationUI;

        // Game Objects and References
        private GameObject pickupCircle;
        private GameObject dropoffCircle;
        private PlayerHUD playerHUD = null;
        private Il2Cpp.Address lastClosestBuildingAddress = null;
        private string currentNeighborhood = "Unknown";
        private readonly Dictionary<Il2Cpp.Address, Vector3> buildingPositions = new Dictionary<Il2Cpp.Address, Vector3>();

        // Vehicle
        private VehicleController lastDetectedVehicle = null;
        private string lastVehicleTypeName = null;
        private VehicleCategory currentVehicleCategory = VehicleCategory.Standard;
        private float? maxVehicleSpeed = null;
        private float? vehiclePrice = null;
        private float? enginePower = null;
        private bool autoParkSupported = false;
        private bool isCheapCar = false;

        // Driving State Tracking
        private float currentSpeed = 0f;
        private float previousSpeed = 0f;
        private float previousTime = 0f;
        private float smoothDrivingTime = 0f;
        private bool isSpeeding = false;
        private bool isCollision = false;
        private bool hasSpeedLimiterModule = false;
        private float gameSpeedLimit = float.MaxValue;
        private float dynamicSpeedLimit = 50f;
        private float speedingTolerance = 5f;
        private float suddenStopCooldown = 0f;
        private float lastCollisionTime = 0f;
        private float lastSuddenStopTime = -100f;
        private float lastSpeedingNotificationTime = -100f;
        private float timeAtPickupAddress = 0f;

        // Timing and Intervals
        private float lastPassengerOfferTime = 0f;
        private float lastJobCompletionTime = -UberJobCooldown;
        private float lastConversationTime = 0f;

        // Passenger Logic
        private BussinesTypeNamePool businessTypePool = new BussinesTypeNamePool();
        private DrivingStateDialogues drivingDialogues;
        private bool conversationShown = false;
        private bool ratingPending = false;
        private bool hadRegularConversation = false;
        private int conversationCount = 0;
        private bool isPeakHours = false;
        private System.Random conversationRandom = new System.Random();
        private bool hasShownVehicleComment = false;
        private bool hasQueuedSpecialTouristComment = false;

        // Player State
        private Gender playerGender;
        private CharacterData playerCharacterData;
        private string playerName;

        #endregion

        #region Initialization
        public override void OnInitializeMelon()
        {
            passengerPool = new Queue<UberPassenger>();
            MelonLogger.Msg("Uber Mod Big Ambitions - loaded successfully!");
            LoadDriverStats();
            InitializeTipSystem();
            MelonCoroutines.Start(WaitForGameLoad());
            drivingDialogues = new DrivingStateDialogues();
        }

        private void InitializeComponents()
        {
            uiManager = new UberUIManager();
            uiManager.InitializeUI();
            uiManager.OnAcceptPassenger += OnAcceptPassenger;
            uiManager.OnCancelClicked += OnCancelButtonClicked;
            uiManager.SetUIVisible(false);

            notificationUI = new UberNotificationUI();
            notificationUI.Initialize();
        }

        private void InitializeTipSystem()
        {
            passengerTypeTipRates[PassengerType.Business] = 0.25f;
            passengerTypeTipRates[PassengerType.Tourist] = 0.20f;
            passengerTypeTipRates[PassengerType.Party] = 0.15f;
            passengerTypeTipRates[PassengerType.Silent] = 0.10f;
            passengerTypeTipRates[PassengerType.Regular] = 0.15f;

            neighborhoodTipRates["Midtown"] = 0.25f;
            neighborhoodTipRates["LowerManhattan"] = 0.15f;
            neighborhoodTipRates["HellsKitchen"] = 0.15f;
            neighborhoodTipRates["GarmentDistrict"] = 0.10f;
            neighborhoodTipRates["MurrayHill"] = 0.20f;

            if (driverStats.tipsByPassengerType == null)
            {
                driverStats.tipsByPassengerType = new Dictionary<PassengerType, float>();
            }

            foreach (PassengerType type in System.Enum.GetValues(typeof(PassengerType)))
            {
                if (!driverStats.tipsByPassengerType.ContainsKey(type))
                {
                    driverStats.tipsByPassengerType[type] = 0f;
                }
            }
        }

        private IEnumerator WaitForGameLoad()
        {
            while (CityManager.Instance == null || CityManager.Instance.CityBuildingControllersDictionary == null)
            {
                yield return new WaitForSeconds(0.5f);
            }

            while (playerHUD == null)
            {
                playerHUD = GameObject.FindObjectOfType<PlayerHUD>();
                yield return new WaitForSeconds(0.5f);
            }

            while (GameObject.FindObjectOfType<EventSystem>() == null)
            {
                MelonLogger.Warning("EventSystem not yet available, waiting...");
                yield return new WaitForSeconds(0.5f);
            }

            yield return LoadPlayerCharacterData();
            LoadAddressData();

            if (addresses.Count == 0)
            {
                yield return new WaitForSeconds(10f);
                LoadAddressData();
            }

            InitializeComponents();

            MelonCoroutines.Start(EfficientUberJobRoutine());
            MelonCoroutines.Start(TimeCheckRoutine());
        }

        private IEnumerator LoadPlayerCharacterData()
        {
            const float timeout = 15f;
            float elapsed = 0f;

            while (playerCharacterData == null && elapsed < timeout)
            {
                try
                {
                    playerCharacterData = Il2CppHelpers.PlayerHelper.GetPlayerCharacterData();
                }
                catch
                {
                    // Retry silently
                }

                if (playerCharacterData == null)
                {
                    yield return new WaitForSeconds(0.5f);
                    elapsed += 0.5f;
                }
            }

            try
            {
                if (playerCharacterData != null)
                {
                    var rawGender = playerCharacterData.gender;

                    playerGender = rawGender switch
                    {
                        Il2CppBigAmbitions.Characters.Gender.Male => Gender.Male,
                        Il2CppBigAmbitions.Characters.Gender.Female => Gender.Female,
                        _ => Gender.Male
                    };

                    if (rawGender != Il2CppBigAmbitions.Characters.Gender.Male &&
                        rawGender != Il2CppBigAmbitions.Characters.Gender.Female)
                    {
                    }

                    playerName = playerCharacterData.name ?? "Driver";
                }
                else
                {
                    playerGender = Gender.Male;
                    playerName = "Driver";
                }
            }
            catch (Exception ex)
            {
                playerGender = Gender.Male;
                playerName = "Driver";
            }

            if (string.IsNullOrEmpty(playerName))
            {
                yield return LoadPlayerNameFromCharacterInfo();
            }
        }

        private IEnumerator LoadPlayerNameFromCharacterInfo()
        {
            const float timeout = 15f;
            float elapsed = 0f;
            CharacterInfo characterInfo = null;

            while (characterInfo == null && elapsed < timeout)
            {
                characterInfo = GameObject.FindObjectOfType<CharacterInfo>();
                if (characterInfo == null)
                {
                    yield return new WaitForSeconds(0.5f);
                    elapsed += 0.5f;
                }
            }

            if (characterInfo != null && characterInfo.characterName != null)
            {
                playerName = characterInfo.characterName.text;
            }

            if (string.IsNullOrEmpty(playerName))
            {
                playerName = "Driver";
            }
        }

        private void LoadAddressData()
        {
            try
            {
                addresses.Clear();
                buildingPositions.Clear();
                var cityManager = CityManager.Instance;

                foreach (var pair in cityManager.CityBuildingControllersDictionary)
                {
                    var gameAddress = pair.Key;
                    var buildingController = pair.Value;
                    var buildingPos = buildingController.transform.position;

                    if (buildingPositions.ContainsKey(gameAddress))
                    {
                        continue;
                    }

                    buildingPositions[gameAddress] = buildingPos;

                    var buildingRegistration = buildingController.buildingRegistration;
                    string businessName = "Unknown Business";
                    string buildingType = "Unknown";
                    BusinessTypeName businessType = BusinessTypeName.Empty;
                    string neighborhood = "Unknown Area";

                    if (buildingRegistration != null)
                    {
                        try
                        {
                            businessName = buildingRegistration.BusinessName ?? "Unknown Business";
                            var buildingTypeObj = buildingRegistration.GetBuildingType();
                            buildingType = buildingTypeObj != null ? buildingTypeObj.ToString() : "Unknown";
                            businessType = buildingRegistration.businessTypeName;
                            var neighborhoodObj = buildingRegistration.GetNeighborhood();
                            neighborhood = neighborhoodObj != null ? neighborhoodObj.ToString() : "Unknown Area";

                            if (buildingType.ToLower() is "residential" or "warehouse" or "special")
                            {
                                businessType = BusinessTypeName.Empty;
                                businessName = "Unknown Business";
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }

                    if (addresses.Any(a => a.gameAddress == gameAddress))
                    {
                        continue;
                    }

                    addresses.Add(new Address
                    {
                        gameAddress = gameAddress,
                        address = businessName,
                        neighborhood = neighborhood,
                        businessName = businessName,
                        businessType = businessType,
                        buildingType = buildingType,
                        totalSize = 0,
                        customerCapacity = 0,
                        trafficIndex = 0,
                        parkingZone = "Unknown"
                    });
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void LoadDriverStats()
        {
            try
            {
                string saveDir = Path.Combine(MelonEnvironment.UserDataDirectory, "UberJobMod");
                string savePath = Path.Combine(saveDir, "DriverStats.json");

                if (!Directory.Exists(saveDir))
                {
                    Directory.CreateDirectory(saveDir);
                }

                if (File.Exists(savePath))
                {
                    string json = File.ReadAllText(savePath);
                    driverStats = JsonConvert.DeserializeObject<DriverStats>(json);
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error loading driver stats: {ex.Message}");
                driverStats = new DriverStats();
            }
        }
        #endregion

        #region Coroutines
        private IEnumerator EfficientUberJobRoutine()
        {
            float idleTimeWithoutJob = 0f;
            float lastDrivingStateUpdate = 0f;

            while (true)
            {
                float waitTime = uberJobActive ? 0.5f : 1f;
                bool shouldContinue = false;

                if (showUberUI || uberJobActive)
                {
                    try
                    {
                        UpdateNeighborhood();
                        VehicleController playerVehicle = GetPlayerVehicle();

                        if (isCompletingJob)
                        {
                            shouldContinue = true;
                        }
                        else if (!uberJobActive)
                        {
                            if (passengerPool == null)
                            {
                                passengerPool = new Queue<UberPassenger>();
                            }

                            Queue<UberPassenger> tempQueue = new Queue<UberPassenger>();
                            int originalCount = passengerPool.Count;

                            for (int i = 0; i < originalCount; i++)
                            {
                                if (passengerPool.Count == 0)
                                {
                                    break;
                                }

                                UberPassenger passenger = passengerPool.Dequeue();
                                if (passenger == null)
                                {
                                    continue;
                                }

                                passenger.waitTime += waitTime;

                                if (passenger.waitTime <= MaxPoolTime)
                                {
                                    tempQueue.Enqueue(passenger);
                                }
                                else
                                {
                                    string passengerName = passenger.passengerName ?? "Unknown Passenger";
                                }
                            }
                            passengerPool = tempQueue;

                            if (passengerPool.Count < 5 && Time.time - lastPassengerOfferTime > PassengerOfferInterval)
                            {
                                GenerateNewPassengerForPool();
                                lastPassengerOfferTime = Time.time;
                            }
                        }
                        else if (currentPassenger != null && !currentPassenger.isPickedUp)
                        {
                            currentPassenger.waitTime += waitTime;
                            if (currentPassenger.waitTime > MaxWaitingTime && UnityEngine.Random.Range(0, 100) < 5)
                            {
                                PassengerCancelRide("The passenger got tired of waiting and cancelled the ride.");
                            }
                        }

                        if (uberJobActive && currentPassenger != null)
                        {
                            if (!currentPassenger.isPickedUp)
                            {
                                Il2CppNWH.VehiclePhysics2.Damage.DamageHandler damageHandler = playerVehicle.GetComponent<Il2CppNWH.VehiclePhysics2.Damage.DamageHandler>();
                                HandlePickup(playerVehicle, damageHandler);
                            }
                            else
                            {
                                HandleDropoff(playerVehicle);
                                if (currentPassenger != null && currentPassenger.isPickedUp && !conversationShown && Time.time - lastConversationTime > ConversationInterval)
                                {
                                    ShowPassengerConversation();
                                    lastConversationTime = Time.time;
                                }
                            }

                            if (playerVehicle != null && currentPassenger != null && currentPassenger.isPickedUp && Time.time - lastDrivingStateUpdate >= 0.2f)
                            {
                                UpdateDrivingState(playerVehicle);
                                lastDrivingStateUpdate = Time.time;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Error($"Error in EfficientUberJobRoutine: {ex.Message}\nStack: {ex.StackTrace}");
                    }
                }

                if (shouldContinue)
                {
                    yield return new WaitForSeconds(waitTime);
                    continue;
                }
                yield return new WaitForSeconds(waitTime);
            }
        }

        private IEnumerator TimeCheckRoutine()
        {
            while (true)
            {
                try
                {
                    int currentHour = Il2Cpp.TimeHelper.CurrentHour;
                    isPeakHours = (currentHour >= 7 && currentHour <= 9) || (currentHour >= 16 && currentHour <= 19);

                    if (isPeakHours)
                    {
                    }
                }
                catch (Exception ex)
                {
                }

                yield return new WaitForSeconds(60f);
            }
        }

        private System.Collections.IEnumerator ResetConversationFlag()
        {
            yield return new WaitForSeconds(15f);
            conversationShown = false;
        }

        private IEnumerator RequestRatingAfterDelay(float delay, UberPassenger passenger)
        {
            yield return new WaitForSeconds(delay);

            float baseRating = UnityEngine.Random.Range(3.5f, 5.1f);
            float ratingDeduction = 0f;
            List<string> feedbackReasons = new List<string>();

            if (passenger.waitTime > 120f)
            {
                float waitDeduction = UnityEngine.Random.Range(0.3f, 0.7f);
                ratingDeduction += waitDeduction;
                feedbackReasons.Add("a late pickup");
            }

            if (passenger.suddenStopCount > 0)
            {
                float stopDeduction = passenger.suddenStopCount * 0.1f;
                ratingDeduction += stopDeduction;
                feedbackReasons.Add("sudden stops");
            }

            if (passenger.totalSpeedingTime > 620f && (!isCheapCar || maxVehicleSpeed > dynamicSpeedLimit * 1.3f))
            {
                float speedDeduction = Mathf.Min(0.3f, passenger.totalSpeedingTime / 100f) * (isCheapCar ? 0.5f : 1f);
                ratingDeduction += speedDeduction;
                feedbackReasons.Add("speeding");
            }

            if (passenger.collisionCount > 0)
            {
                float collisionDeduction = passenger.collisionCount * 0.5f;
                ratingDeduction += collisionDeduction;
                feedbackReasons.Add(passenger.collisionCount == 1 ? "a crash" : "multiple crashes");
            }

            ratingDeduction = Mathf.Min(ratingDeduction, 3.5f);
            float finalRating = Mathf.Clamp(baseRating - ratingDeduction, 1f, 5f);

            if (passenger.collisionCount > 0)
            {
                finalRating = Mathf.Min(finalRating, 4.0f);
            }

            if (passenger.collisionCount >= 3)
            {
                finalRating = Mathf.Min(finalRating, 2.5f);
            }

            string feedbackMessage = feedbackReasons.Count == 1
            ? $"Because of <color=yellow>{feedbackReasons[0]}</color>."
            : feedbackReasons.Count > 1
                ? $"Because of <color=yellow>{string.Join("</color>, <color=yellow>", feedbackReasons.Take(feedbackReasons.Count - 1))}</color> and <color=yellow>{feedbackReasons.Last()}</color>."
                : "";

            driverStats.ratings.Add(finalRating);
            driverStats.averageRating = driverStats.ratings.Average();

            string notificationMessage = $"{passenger.passengerName} rated you {finalRating:F1} ★" +
                                        (string.IsNullOrEmpty(feedbackMessage) ? "" : $"\n{feedbackMessage}");
            NotificationType notificationType = finalRating >= 4f ? NotificationType.Success :
                                               finalRating >= 3f ? NotificationType.Info :
                                               NotificationType.Warning;

            ShowNotification(notificationMessage, notificationType);
            ratingPending = false;
            SaveDriverStats();
            lastPassengerOfferTime = Time.time - PassengerOfferInterval + 15f;
        }

        private IEnumerator HideUIAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            showUberUI = false;
            uiManager.SetUIVisible(false);
        }

        private System.Collections.IEnumerator ShowSpecialTouristConversation()
        {
            yield return new WaitForSeconds(1f);

            List<string> touristCommentsMale = new List<string>
            {
                "Tacos here legit?",
                "Any cool spots for photos, dude?",
                "This city’s wild, man—any hidden gems?",
                "Where’s the best coffee around here, pal?",
                "Got any local festival tips, bro?"
            };

            List<string> touristCommentsFemale = new List<string>
            {
                "Tacos here legit?",
                "Where can I snap some cute pics, hon?",
                "This place is amazing—any secret spots, dear?",
                "Best place for a coffee date, amiga?",
                "Any fun local events coming up, love?"
            };

            var commentList = playerGender == Gender.Male ? touristCommentsMale : touristCommentsFemale;
            string key = $"tourist_special_{playerGender}";
            string comment = commentList[ConversationTracker.GetRandomLineIndex(commentList, key)];

            ShowNotification($"{currentPassenger.passengerName}: \"{comment}\"", NotificationType.Info, 15f);
            currentPassenger.hadSpecialConversation = true;
            conversationShown = true;
            conversationCount++;

            MelonCoroutines.Start(ResetConversationFlag());
        }
        #endregion

        #region Event Handlers
        public override void OnUpdate()
        {
            if (uiManager == null)
            {
                return;
            }

            if (uberJobActive && currentPassenger != null)
            {
                UpdateNeighborhood();
            }

            if (showUberUI)
            {
                UpdateUITitle();
                uiManager.UpdateUI(driverStats, currentPassenger, passengerPool, isPeakHours, uberJobActive);
            }

            if (Input.GetKeyDown(KeyCode.F3))
            {
                ToggleUI();
            }
        }

        private void UpdateUITitle()
        {
            VehicleController playerVehicle = GetPlayerVehicle();

            if (playerVehicle != null && uiManager.titleText != null)
            {
                string vehicleName = playerVehicle.GetName()?.ToString();

                if (!string.IsNullOrEmpty(vehicleName))
                {
                    string newTitle = $"Uber | {playerName} | {vehicleName}";

                    if (uiManager.titleText.text != newTitle)
                    {
                        uiManager.titleText.text = newTitle;
                    }
                }
            }
        }

        private void ToggleUI()
        {
            showUberUI = !showUberUI;
            uiManager.SetUIVisible(showUberUI);

            if (showUberUI && !uberJobActive && passengerPool.Count == 0 &&
                Time.time - lastJobCompletionTime > UberJobCooldown)
            {
                GenerateNewPassengerForPool();
                lastPassengerOfferTime = Time.time;
            }
        }

        private void OnAcceptPassenger(UberPassenger passenger)
        {
            currentPassenger = passenger;
            uberJobActive = true;
            string notificationMessage = $"Ride accepted!\n\nDrive to pick up <color=yellow>{currentPassenger.passengerName}</color>, at <b><color=white>{currentPassenger.pickupAddress.DisplayName}</color></b> <color=yellow>({currentPassenger.pickupAddress.neighborhood})</color>";
            ShowNotification(notificationMessage);
            CityManager.Instance.FindBuildingAndSetGuider(currentPassenger.pickupAddress.gameAddress, true);
            SpawnPickupCircle();
            lastCollisionTime = Time.time;
            lastSuddenStopTime = -100f;
            suddenStopCooldown = 0f;
            isCollision = false;

            Queue<UberPassenger> tempQueue = new Queue<UberPassenger>();
            while (passengerPool.Count > 0)
            {
                var queuedPassenger = passengerPool.Dequeue();
                if (queuedPassenger != passenger)
                {
                    tempQueue.Enqueue(queuedPassenger);
                }
            }
            passengerPool = tempQueue;
        }

        private void OnCancelButtonClicked()
        {
            driverStats.cancelledRides++;
            uberJobActive = false;
            CleanupCircles();
            currentPassenger = null;
            showUberUI = true;
            uiManager.SetUIVisible(showUberUI);
            ShowNotification("Uber ride canceled by driver.");
            SaveDriverStats();
            uiManager.UpdateUI(driverStats, currentPassenger, passengerPool, isPeakHours, uberJobActive);
            lastPassengerOfferTime = Time.time - PassengerOfferInterval + 15f;
        }

        public override void OnDeinitializeMelon()
        {
            uiManager?.DestroyUI();
            CleanupCircles();
        }
        #endregion

        #region Passenger Management
        private void GenerateNewPassengerForPool()
        {
            List<string> maleConversationLines = new List<string>();
            List<string> femaleConversationLines = new List<string>();

            if (addresses.Count < 2) return;

            int currentHour = Il2Cpp.TimeHelper.CurrentHour;

            PassengerType passengerType;
            float rand = UnityEngine.Random.Range(0f, 1f);
            if (currentHour >= 5 && currentHour < 9)
            {
                if (rand < 0.5f) passengerType = PassengerType.Business;
                else if (rand < 0.8f) passengerType = PassengerType.Regular;
                else if (rand < 0.95f) passengerType = PassengerType.Silent;
                else passengerType = PassengerType.Tourist;
            }
            else if (currentHour >= 9 && currentHour < 17)
            {
                if (rand < 0.3f) passengerType = PassengerType.Business;
                else if (rand < 0.7f) passengerType = PassengerType.Regular;
                else if (rand < 0.8f) passengerType = PassengerType.Silent;
                else passengerType = PassengerType.Tourist;
            }
            else if (currentHour >= 17 && currentHour < 22)
            {
                if (rand < 0.2f) passengerType = PassengerType.Business;
                else if (rand < 0.55f) passengerType = PassengerType.Regular;
                else if (rand < 0.65f) passengerType = PassengerType.Silent;
                else if (rand < 0.8f) passengerType = PassengerType.Tourist;
                else passengerType = PassengerType.Party;
            }
            else
            {
                if (rand < 0.05f) passengerType = PassengerType.Business;
                else if (rand < 0.25f) passengerType = PassengerType.Regular;
                else if (rand < 0.45f) passengerType = PassengerType.Silent;
                else if (rand < 0.6f) passengerType = PassengerType.Tourist;
                else passengerType = PassengerType.Party;
            }

            // Get preferred business types for the passenger
            List<BusinessTypeName> preferredBusinessTypes = businessTypePool.preferredBusinessTypes[passengerType];

            // Filter residential and business addresses
            List<Address> residentialAddresses = addresses.Where(a => a.buildingType.ToLower() == "residential").ToList();
            if (residentialAddresses.Count == 0)
            {
                MelonLogger.Warning("No Residential addresses found. Falling back to all addresses.");
                residentialAddresses = addresses.ToList();
            }

            List<Address> preferredBusinessAddresses;
            if (preferredBusinessTypes.Count > 0 && passengerType != PassengerType.Silent)
            {
                if (currentHour >= 22 || currentHour < 6)
                {
                    if (passengerType == PassengerType.Party)
                    {
                        preferredBusinessAddresses = addresses.Where(a => preferredBusinessTypes.Contains(a.businessType)).ToList();
                    }
                    else
                    {
                        preferredBusinessAddresses = addresses.Where(a =>
                            (a.businessType == BusinessTypeName.Nightclub ||
                             a.businessType == BusinessTypeName.Casino ||
                             a.businessType == BusinessTypeName.Hospital ||
                             a.businessType == BusinessTypeName.GasStation ||
                             a.businessType == BusinessTypeName.Supermarket) &&
                            a.buildingType.ToLower() != "residential").ToList();
                    }
                }
                else
                {
                    preferredBusinessAddresses = addresses.Where(a => preferredBusinessTypes.Contains(a.businessType)).ToList();
                }
            }
            else
            {
                if (currentHour >= 22 || currentHour < 6)
                {
                    preferredBusinessAddresses = addresses.Where(a =>
                        (a.businessType == BusinessTypeName.Nightclub ||
                         a.businessType == BusinessTypeName.Casino ||
                         a.businessType == BusinessTypeName.Hospital ||
                         a.businessType == BusinessTypeName.GasStation ||
                         a.businessType == BusinessTypeName.Supermarket) &&
                        a.buildingType.ToLower() != "residential").ToList();
                }
                else
                {
                    preferredBusinessAddresses = addresses.Where(a =>
                        a.businessType != BusinessTypeName.Empty &&
                        a.buildingType.ToLower() != "residential").ToList();
                }
            }

            if (preferredBusinessAddresses.Count == 0)
            {
                MelonLogger.Warning($"No addresses match preferred business types for {passengerType} at hour {currentHour}. Falling back.");
                preferredBusinessAddresses = addresses.Where(a => a.buildingType.ToLower() != "residential").ToList();
                if (preferredBusinessAddresses.Count == 0)
                {
                    MelonLogger.Warning("No non-Residential addresses available. Using all addresses.");
                    preferredBusinessAddresses = addresses.ToList();
                }
            }

            // Determine pickup address
            Address pickupAddress;
            float residentialPickupChance;
            if (currentHour >= 5 && currentHour < 10)
                residentialPickupChance = 0.7f;
            else if (currentHour >= 10 && currentHour < 17)
                residentialPickupChance = 0.6f;
            else if (currentHour >= 17 && currentHour < 22)
                residentialPickupChance = 0.5f;
            else
                residentialPickupChance = 0.4f;

            if (passengerType == PassengerType.Party && (currentHour >= 22 || currentHour < 3))
                residentialPickupChance *= 0.75f;

            bool isResidentialPickup = UnityEngine.Random.Range(0f, 1f) < residentialPickupChance;

            if (isResidentialPickup && residentialAddresses.Count > 0)
            {
                pickupAddress = residentialAddresses[UnityEngine.Random.Range(0, residentialAddresses.Count)];
            }
            else
            {
                pickupAddress = preferredBusinessAddresses[UnityEngine.Random.Range(0, preferredBusinessAddresses.Count)];
            }

            // Determine dropoff address
            Address dropoffAddress;
            float residentialDropoffChance;
            if (currentHour >= 5 && currentHour < 10)
                residentialDropoffChance = 0.3f;
            else if (currentHour >= 10 && currentHour < 17)
                residentialDropoffChance = 0.4f;
            else if (currentHour >= 17 && currentHour < 22)
                residentialDropoffChance = 0.5f;
            else
                residentialDropoffChance = 0.6f;

            if (pickupAddress.buildingType.ToLower() == "residential")
                residentialDropoffChance *= 0.75f;

            bool isResidentialDropoff = UnityEngine.Random.Range(0f, 1f) < residentialDropoffChance;

            List<Address> possibleDropoffAddresses;
            if (isResidentialDropoff)
            {
                possibleDropoffAddresses = residentialAddresses.Where(a => a.gameAddress != pickupAddress.gameAddress).ToList();
            }
            else
            {
                if ((passengerType == PassengerType.Party || passengerType == PassengerType.Regular) && !isResidentialPickup)
                {
                    BusinessTypeName pickupBusinessType = pickupAddress.businessType;
                    possibleDropoffAddresses = preferredBusinessAddresses
                        .Where(a => a.gameAddress != pickupAddress.gameAddress && a.businessType != pickupBusinessType)
                        .ToList();
                    if (possibleDropoffAddresses.Count == 0)
                    {
                        // [HOTFIX] leave it, because the party passenger are only have residential after the Nightclub.
                        possibleDropoffAddresses = preferredBusinessAddresses.Where(a => a.gameAddress != pickupAddress.gameAddress).ToList();
                    }
                }
                else
                {
                    possibleDropoffAddresses = preferredBusinessAddresses.Where(a => a.gameAddress != pickupAddress.gameAddress).ToList();
                }
            }

            if (possibleDropoffAddresses.Count == 0)
            {
                possibleDropoffAddresses = addresses.Where(a => a.gameAddress != pickupAddress.gameAddress).ToList();
                if (possibleDropoffAddresses.Count == 0)
                {
                    return;
                }
            }
            dropoffAddress = possibleDropoffAddresses[UnityEngine.Random.Range(0, possibleDropoffAddresses.Count)];

            if (!buildingPositions.ContainsKey(pickupAddress.gameAddress) || !buildingPositions.ContainsKey(dropoffAddress.gameAddress))
            {
                return;
            }

            // Calculate fare
            Vector3 pickupPos = buildingPositions[pickupAddress.gameAddress];
            Vector3 dropoffPos = buildingPositions[dropoffAddress.gameAddress];
            float distance = Vector3.Distance(pickupPos, dropoffPos);
            float fareMultiplier = 1.0f;
            if ((currentHour >= 7 && currentHour < 10) || (currentHour >= 16 && currentHour < 19))
                fareMultiplier = 1.3f;
            else if (currentHour >= 22 || currentHour < 5)
                fareMultiplier = 1.25f;
            if (isPeakHours)
                fareMultiplier *= PeakMultiplier;
            float fare = CalculateFare(distance) * fareMultiplier;

            // Generate passenger details
            Gender passengerGender = UnityEngine.Random.Range(0, 2) == 0 ? Gender.Male : Gender.Female;
            string passengerName = GeneratePassengerName(passengerGender);

            // Get location display names
            string pickupDisplayName = GetDisplayLocationName(pickupAddress.buildingType, pickupAddress.businessType.ToString());
            string dropoffDisplayName = GetDisplayLocationName(dropoffAddress.buildingType, dropoffAddress.businessType.ToString());
            string pickupLocationType = pickupAddress.buildingType.ToLower() == "residential" ? "Residential" : pickupAddress.businessType.ToString();
            string dropoffLocationType = dropoffAddress.buildingType.ToLower() == "residential" ? "Residential" : dropoffAddress.businessType.ToString();

            // Generate conversation lines
            List<string> conversationLines = new List<string>();
            bool isSameBusinessType = !isResidentialPickup && !isResidentialDropoff && pickupLocationType == dropoffLocationType;
            bool isNight = currentHour >= 22 || currentHour < 6;

            if (passengerType == PassengerType.Business)
            {
                string dynamicKey = isNight ? "dynamic_night" : (isSameBusinessType ? "dynamic_day_same_type" : "dynamic_day");
                if (PassengerDialogues.Comments[passengerType].ContainsKey(dynamicKey))
                {
                    foreach (var line in PassengerDialogues.Comments[passengerType][dynamicKey][Gender.Male])
                    {
                        string formattedLine = string.Format(line, pickupDisplayName, dropoffDisplayName);
                        maleConversationLines.Add(formattedLine);
                    }
                    foreach (var line in PassengerDialogues.Comments[passengerType][dynamicKey][Gender.Female])
                    {
                        string formattedLine = string.Format(line, pickupDisplayName, dropoffDisplayName);
                        femaleConversationLines.Add(formattedLine);
                    }
                }
            }
            else if (passengerType == PassengerType.Tourist)
            {
                string dynamicKey = isResidentialDropoff ? "dynamic_to_residential" : "dynamic";
                if (PassengerDialogues.Comments[passengerType].ContainsKey(dynamicKey))
                {
                    foreach (var line in PassengerDialogues.Comments[passengerType][dynamicKey][Gender.Male])
                    {
                        string formattedLine = string.Format(line, pickupDisplayName, dropoffDisplayName);
                        maleConversationLines.Add(formattedLine);
                    }
                    foreach (var line in PassengerDialogues.Comments[passengerType][dynamicKey][Gender.Female])
                    {
                        string formattedLine = string.Format(line, pickupDisplayName, dropoffDisplayName);
                        femaleConversationLines.Add(formattedLine);
                    }
                }
            }
            else if (passengerType == PassengerType.Party)
            {
                string dynamicKey = isSameBusinessType ? "dynamic_same_type" : "dynamic";
                if (PassengerDialogues.Comments[passengerType].ContainsKey(dynamicKey))
                {
                    foreach (var line in PassengerDialogues.Comments[passengerType][dynamicKey][Gender.Male])
                    {
                        string formattedLine = string.Format(line, pickupDisplayName, dropoffDisplayName);
                        maleConversationLines.Add(formattedLine);
                    }
                    foreach (var line in PassengerDialogues.Comments[passengerType][dynamicKey][Gender.Female])
                    {
                        string formattedLine = string.Format(line, pickupDisplayName, dropoffDisplayName);
                        femaleConversationLines.Add(formattedLine);
                    }
                }
            }
            else if (passengerType == PassengerType.Regular || passengerType == PassengerType.Silent)
            {
                if (PassengerDialogues.Comments[passengerType].ContainsKey("dynamic"))
                {
                    foreach (var line in PassengerDialogues.Comments[passengerType]["dynamic"][Gender.Male])
                    {
                        string formattedLine = string.Format(line, pickupDisplayName, dropoffDisplayName);
                        maleConversationLines.Add(formattedLine);
                    }
                    foreach (var line in PassengerDialogues.Comments[passengerType]["dynamic"][Gender.Female])
                    {
                        string formattedLine = string.Format(line, pickupDisplayName, dropoffDisplayName);
                        femaleConversationLines.Add(formattedLine);
                    }
                }
            }

            // as-is
            conversationLines.AddRange(PassengerDialogues.Comments[passengerType]["regular"][passengerGender]);

            // Create passenger
            var newPassenger = new UberPassenger
            {
                pickupAddress = pickupAddress,
                dropoffAddress = dropoffAddress,
                pickupLocation = pickupPos,
                dropoffLocation = dropoffPos,
                fare = fare,
                isPickedUp = false,
                pickupTime = -1f,
                distanceToDestination = distance,
                passengerType = passengerType,
                passengerName = passengerName,
                conversationLines = conversationLines.ToArray(),
                collisionCount = 0,
                suddenStopCount = 0,
                totalSpeedingTime = 0f,
                gender = passengerGender,
                dynamicLineCount = conversationLines.Count(l => l.Contains(pickupDisplayName) || l.Contains(dropoffDisplayName))
            };

            newPassenger.maleConversationLines = maleConversationLines.ToArray();
            newPassenger.femaleConversationLines = femaleConversationLines.ToArray();
            newPassenger.conversationLines = conversationLines.ToArray();
            newPassenger.dynamicLineCount = maleConversationLines.Count;

            passengerPool.Enqueue(newPassenger);
        }

        private string GeneratePassengerName(Gender gender)
        {
            string firstName = gender == Gender.Male ?
                PassengerDialogues.MaleFirstNames[UnityEngine.Random.Range(0, PassengerDialogues.MaleFirstNames.Length)] :
                PassengerDialogues.FemaleFirstNames[UnityEngine.Random.Range(0, PassengerDialogues.FemaleFirstNames.Length)];
            string lastName = PassengerDialogues.LastNames[UnityEngine.Random.Range(0, PassengerDialogues.LastNames.Length)];
            return $"{firstName} {lastName}";
        }

        private string GetDisplayLocationName(string buildingType, string businessType)
        {
            if (!string.IsNullOrEmpty(buildingType) && buildingType.ToLower() == "residential")
            {
                return conversationRandom.Next(0, 2) == 0 ? "Apartment" : "Home";
            }

            if (!string.IsNullOrEmpty(businessType))
            {
                switch (businessType)
                {
                    case "WebDevelopmentAgency":
                        return "Web Development Agency";
                    case "LawFirm":
                        return "Law Firm";
                    case "Nightclub":
                        return "Night Club";
                    case "GasStation":
                        return "Gas Station";
                    default:
                        string result = "";
                        for (int i = 0; i < businessType.Length; i++)
                        {
                            if (i > 0 && char.IsUpper(businessType[i]))
                            {
                                result += " ";
                            }
                            result += businessType[i];
                        }
                        return result;
                }
            }

            return "Unknown Location";
        }
        private void ShowPassengerConversation()
        {
            if (currentPassenger == null || !currentPassenger.isPickedUp)
            {
                return;
            }

            if (conversationCount == 0)
            {
                hasShownVehicleComment = false;
                hasQueuedSpecialTouristComment = false;
            }

            if (currentPassenger.passengerType == PassengerType.Silent &&
                conversationRandom.Next(0, 100) < 90 &&
                Time.time - lastSuddenStopTime >= 10f)
            {
                return;
            }

            string commentType;
            bool isDynamicLine = false;

            if (conversationCount == 0 || !hadRegularConversation)
            {
                commentType = "regular";
                hadRegularConversation = true;
            }
            else
            {
                if (isCollision && currentPassenger.collisionCount > 0 && Time.time - lastCollisionTime < 10f && Time.time >= currentPassenger.pickupTime)
                {
                    commentType = "collision";
                }
                else if (Time.time - lastSuddenStopTime < 8f)
                {
                    commentType = "sudden_stop";
                }
                else if (isSpeeding && currentPassenger.totalSpeedingTime > 15f && currentSpeed <= (maxVehicleSpeed ?? 100f) * 0.9f)
                {
                    commentType = "speeding";
                }
                else if (smoothDrivingTime > 60f)
                {
                    commentType = "smooth";
                    smoothDrivingTime = 0f;
                }
                else if (currentSpeed < 5f && Time.time - lastSuddenStopTime > 10f)
                {
                    var brakes = GetPlayerVehicle()?.gameObject?.GetComponent<Il2CppNWH.VehiclePhysics2.Brakes>();
                    if (brakes != null && brakes._isBraking && autoParkSupported && conversationRandom.Next(0, 100) < 30)
                    {
                        commentType = "parking";
                    }
                    else
                    {
                        commentType = "regular";
                    }
                }
                else
                {
                    commentType = "regular";
                }
            }

            float effectiveEnginePower = enginePower ?? 45f;
            VehicleController playerVehicle = GetPlayerVehicle();
            string vehicleName = playerVehicle?.GetName()?.ToString();
            bool isHighEndVehicle = currentVehicleCategory == VehicleCategory.Luxury || currentVehicleCategory == VehicleCategory.Performance;

            string greeting = GetPassengerGreeting(commentType);
            bool canFlirt = ShouldUseFlirtyDialogue();

            if (commentType == "regular" && canFlirt && conversationRandom.Next(0, 100) < 15)
            {
                commentType = "flirty";
            }

            if (commentType == "regular" && !hasShownVehicleComment && conversationRandom.Next(0, 100) < 25)
            {
                ShowVehicleComment(greeting, vehicleName, isHighEndVehicle, effectiveEnginePower);
                return;
            }

            if (commentType == "parking")
            {
                ShowParkingComment(greeting, vehicleName, isHighEndVehicle);
                return;
            }

            commentType = AdjustCommentType(commentType);

            string conversation;
            string pickupDisplayName = GetDisplayLocationName(
                currentPassenger.pickupAddress.buildingType,
                currentPassenger.pickupAddress.businessType.ToString());
            string dropoffDisplayName = GetDisplayLocationName(
                currentPassenger.dropoffAddress.buildingType,
                currentPassenger.dropoffAddress.businessType.ToString());
            string pickupLocationType = currentPassenger.pickupAddress.buildingType.ToLower() == "residential"
                ? "Residential"
                : currentPassenger.pickupAddress.businessType.ToString();
            string dropoffLocationType = currentPassenger.dropoffAddress.buildingType.ToLower() == "residential"
                ? "Residential"
                : currentPassenger.dropoffAddress.businessType.ToString();

            bool isSameResidential = pickupLocationType == "Residential" && dropoffLocationType == "Residential";
            bool isSameBusinessType = !isSameResidential && pickupLocationType == dropoffLocationType;
            bool isNight = Il2Cpp.TimeHelper.CurrentHour >= 22 || Il2Cpp.TimeHelper.CurrentHour < 6;

            if (commentType == "regular" && currentPassenger.dynamicLineCount > 0 && conversationRandom.Next(0, 100) < 50)
            {
                string dynamicCommentType = "dynamic";
                if (currentPassenger.passengerType == PassengerType.Business)
                {
                    dynamicCommentType = isNight ? "dynamic_night" : (isSameBusinessType ? "dynamic_day_same_type" : "dynamic_day");
                }
                else if (currentPassenger.passengerType == PassengerType.Party)
                {
                    dynamicCommentType = isSameBusinessType ? "dynamic_same_type" : "dynamic";
                }
                else if (currentPassenger.passengerType == PassengerType.Tourist)
                {
                    dynamicCommentType = dropoffLocationType == "Residential" ? "dynamic_to_residential" : "dynamic";
                }

                var dynamicLines = (playerGender == Gender.Male ? currentPassenger.maleConversationLines : currentPassenger.femaleConversationLines)
                    .Where(line => line.Contains(pickupDisplayName) || line.Contains(dropoffDisplayName))
                    .ToList();

                if (dynamicLines.Count > 0)
                {
                    string key = $"dynamic_{currentPassenger.passengerType}_{playerGender}";
                    conversation = dynamicLines[ConversationTracker.GetRandomLineIndex(dynamicLines, key)];
                    isDynamicLine = true;
                }
                else
                {
                    if (currentPassenger.passengerType == PassengerType.Tourist && !currentPassenger.hadSpecialConversation && !hasQueuedSpecialTouristComment && conversationCount < 2)
                    {
                        MelonCoroutines.Start(ShowSpecialTouristConversation());
                        hasQueuedSpecialTouristComment = true;
                        return;
                    }
                    conversation = PassengerDialogues.GetRandomComment(currentPassenger.passengerType, commentType, playerGender);
                }
            }
            else
            {
                if (currentPassenger.passengerType == PassengerType.Tourist && !currentPassenger.hadSpecialConversation && !hasQueuedSpecialTouristComment && conversationCount < 2)
                {
                    MelonCoroutines.Start(ShowSpecialTouristConversation());
                    hasQueuedSpecialTouristComment = true;
                    return;
                }
                conversation = PassengerDialogues.GetRandomComment(currentPassenger.passengerType, commentType, playerGender);
            }

            if (string.IsNullOrEmpty(conversation))
            {
                return;
            }

            if (commentType == "speeding" || commentType == "sudden_stop" || commentType == "collision")
            {
                currentPassenger.negativeInteractions += (int)1f;
            }
            else if (commentType == "smooth")
            {
                currentPassenger.positiveInteractions += (int)1f;
            }

            if (!isDynamicLine)
            {
                conversation = greeting + conversation;
            }

            ShowNotification($"{currentPassenger.passengerName}: \"{conversation}\"", NotificationType.Info, 15f);
            conversationShown = true;
            conversationCount++;

            MelonCoroutines.Start(ResetConversationFlag());
        }
        private string GetPassengerGreeting(string commentType)
        {
            List<string> greetings;
            if (commentType == "flirty")
            {
                greetings = playerGender == Gender.Male
                    ? new List<string> { "Hey there, ", "Well, hi, ", "Oh, " }
                    : new List<string> { "Hi, sweetie, ", "Hey, love, ", "Oh, dear, " };
            }
            else if (commentType == "speeding" || commentType == "sudden_stop" || commentType == "collision")
            {
                greetings = new List<string> { "Whoa, ", "Hey, ", "Oh no, " };
            }
            else
            {
                greetings = new List<string> { "Hey, ", "Hi, ", "So, " };
            }

            string key = $"greeting_{commentType}_{playerGender}";
            return greetings[ConversationTracker.GetRandomLineIndex(greetings, key)];
        }

        private bool ShouldUseFlirtyDialogue()
        {
            return currentPassenger.passengerType != PassengerType.Silent &&
                   currentPassenger.positiveInteractions > currentPassenger.negativeInteractions &&
                   conversationCount > 0;
        }

        private void ShowVehicleComment(string greeting, string vehicleName, bool isHighEndVehicle, float effectiveEnginePower)
        {
            string vehicleComment;

            if (isHighEndVehicle && !string.IsNullOrEmpty(vehicleName))
            {
                Dictionary<PassengerType, List<string>> highEndCommentsMale = new Dictionary<PassengerType, List<string>>
                {
                    { PassengerType.Business, new List<string>
                        {
                            "{0}This {1} is perfect for my meetings!",
                            "{0}This {1} suits my style, bro!",
                            "{0}Love the class of this {1}, man!"
                        }
                    },
                    { PassengerType.Tourist, new List<string>
                        {
                            "Wow, {1}? This ride’s a trip highlight dude!",
                            "This {1} is awesome, pal!",
                            "{1}? Making memories in this, bro!"
                        }
                    },
                    { PassengerType.Party, new List<string>
                        {
                            "Yo, {1}? This car’s fire, bro!",
                            "This {1} is lit, dude!",
                            "Party vibes with this {1}, man!"
                        }
                    },
                    { PassengerType.Silent, new List<string>
                        {
                            "*nods approvingly at the {1}*",
                            "*glances at the {1} with a slight smile*",
                            "*quietly appreciates the {1}*"
                        }
                    },
                    { PassengerType.Regular, new List<string>
                        {
                            "{0}Nice {1}, I feel special!",
                            "{0}This {1} is awesome, man!",
                            "{0}Great pick with this {1}, bro!"
                        }
                    }
                };

                Dictionary<PassengerType, List<string>> highEndCommentsFemale = new Dictionary<PassengerType, List<string>>
                {
                    { PassengerType.Business, new List<string>
                        {
                            "{0}This {1} is ideal for work, dear!",
                            "{0}This {1} has such elegance, hon!",
                            "{0}Perfect {1} for my schedule, love!"
                        }
                    },
                    { PassengerType.Tourist, new List<string>
                        {
                            "{1}? Loving this ride, hon!",
                            "This {1} is so fun, dear!",
                            "Wow, {1}? Best part of my trip, amiga!"
                        }
                    },
                    { PassengerType.Party, new List<string>
                        {
                            "Hey, {1}? Total vibes, love it!",
                            "This {1} is giving party energy, babe!",
                            "{1}? Obsessed with this ride, hon!"
                        }
                    },
                    { PassengerType.Silent, new List<string>
                        {
                            "*nods approvingly at the {1}*",
                            "*glances at the {1} with a slight smile*",
                            "*quietly appreciates the {1}*"
                        }
                    },
                    { PassengerType.Regular, new List<string>
                        {
                            "{0}Great {1}, love the vibe!",
                            "{0}This {1} feels so nice, dear!",
                            "{0}Amazing {1}, I’m impressed, hon!"
                        }
                    }
                };

                var commentList = playerGender == Gender.Male ? highEndCommentsMale[currentPassenger.passengerType] : highEndCommentsFemale[currentPassenger.passengerType];
                string key = $"vehicle_highend_{currentPassenger.passengerType}_{playerGender}";
                vehicleComment = commentList[ConversationTracker.GetRandomLineIndex(commentList, key)];
                vehicleComment = string.Format(vehicleComment, greeting, vehicleName);
            }
            else
            {
                if (currentVehicleCategory == VehicleCategory.Luxury)
                {
                    List<string> luxuryComments = currentPassenger.passengerType == PassengerType.Business
                        ? (playerGender == Gender.Male
                            ? new List<string> { "{0}This car’s upscale bro!", "{0}Feeling fancy in this, man!", "{0}Luxury suits me, pal!" }
                            : new List<string> { "{0}So luxurious, hon!", "{0}This car’s pure elegance, dear!", "{0}Love the luxury feel, love!" })
                        : new List<string> { "Pure luxury ride!", "This car’s so plush!", "Feels like a dream ride!" };

                    string key = $"vehicle_luxury_{currentPassenger.passengerType}_{playerGender}";
                    vehicleComment = luxuryComments[ConversationTracker.GetRandomLineIndex(luxuryComments, key)];
                    vehicleComment = string.Format(vehicleComment, greeting);
                }
                else if (currentVehicleCategory == VehicleCategory.Economy)
                {
                    List<string> economyComments = currentPassenger.passengerType == PassengerType.Tourist
                        ? (effectiveEnginePower < 40f
                            ? new List<string> { "Vibe’s cool but it’s slow!", "This car’s a bit sluggish, huh?", "Charming, but needs more power!" }
                            : new List<string> { "This car’s got charm!", "Love the quirky feel of this ride!", "This car’s got character!" })
                        : (effectiveEnginePower < 40f
                            ? new List<string> { "{0}We’re crawling!", "{0}This car’s too slow, ugh!", "{0}Feels like we’re barely moving!" }
                            : new List<string> { "{0}Hope it holds up dear!", "{0}This car’s doing its best, I guess!", "{0}Let’s see if this ride makes it!" });

                    string key = $"vehicle_economy_{currentPassenger.passengerType}_{effectiveEnginePower < 40f}_{playerGender}";
                    vehicleComment = economyComments[ConversationTracker.GetRandomLineIndex(economyComments, key)];
                    vehicleComment = string.Format(vehicleComment, greeting);
                }
                else if (currentVehicleCategory == VehicleCategory.Performance)
                {
                    List<string> performanceComments = currentPassenger.passengerType == PassengerType.Party
                        ? (effectiveEnginePower > 80f
                            ? new List<string> { "This car’s a beast, awesome!", "Feel the power in this ride, wow!", "This car’s insane, love it!" }
                            : new List<string> { "Cool ride, crank it up!", "This car’s got potential, let’s roll!", "Nice speed in this one, go faster!" })
                        : new List<string> { "Feels fast just sitting here!", "This car’s got some kick to it!", "Speedy vibe in this ride!" };

                    string key = $"vehicle_performance_{currentPassenger.passengerType}_{effectiveEnginePower > 80f}";
                    vehicleComment = performanceComments[ConversationTracker.GetRandomLineIndex(performanceComments, key)];
                }
                else
                {
                    List<string> defaultComments = autoParkSupported
                        ? new List<string> { "{0}Solid car, neat tech!", "{0}Love the tech in this ride!", "{0}This car’s got cool features!" }
                        : new List<string> { "{0}Good car, gets it done.", "{0}Reliable ride, nice!", "{0}This car’s solid enough!" };

                    string key = $"vehicle_default_{autoParkSupported}_{playerGender}";
                    vehicleComment = defaultComments[ConversationTracker.GetRandomLineIndex(defaultComments, key)];
                    vehicleComment = string.Format(vehicleComment, greeting);
                }
            }

            ShowNotification($"{currentPassenger.passengerName}: \"{vehicleComment}\"", NotificationType.Info, 15f);
            conversationShown = true;
            conversationCount++;

            if (currentVehicleCategory == VehicleCategory.Luxury || currentVehicleCategory == VehicleCategory.Performance || autoParkSupported)
            {
                currentPassenger.positiveInteractions += (int)1f;
            }
            else if (currentVehicleCategory == VehicleCategory.Economy || effectiveEnginePower < 40f)
            {
                currentPassenger.negativeInteractions += (int)0.5f;
            }

            hasShownVehicleComment = true;
            MelonCoroutines.Start(ResetConversationFlag());
        }
        private void ShowParkingComment(string greeting, string vehicleName, bool isHighEndVehicle)
        {
            List<string> parkingComments = new List<string>
            {
                "{0}Nice parking assist!",
                "{0}That auto-park is slick!",
                "{0}Love the parking tech!"
            };

            string key = $"parking_{playerGender}";
            string comment = parkingComments[ConversationTracker.GetRandomLineIndex(parkingComments, key)];
            comment = string.Format(comment, greeting);

            ShowNotification($"{currentPassenger.passengerName}: \"{comment}\"", NotificationType.Info, 15f);
            conversationShown = true;
            conversationCount++;
            currentPassenger.positiveInteractions += (int)1f;

            MelonCoroutines.Start(ResetConversationFlag());
        }

        private string AdjustCommentType(string commentType)
        {
            if (commentType == "regular" && currentPassenger.passengerType == PassengerType.Party && conversationCount > 1)
            {
                string pickupLocationType = currentPassenger.pickupAddress.buildingType.ToLower() == "residential"
                    ? "Residential"
                    : currentPassenger.pickupAddress.businessType.ToString();
                string dropoffLocationType = currentPassenger.dropoffAddress.buildingType.ToLower() == "residential"
                    ? "Residential"
                    : currentPassenger.dropoffAddress.businessType.ToString();

                if (pickupLocationType == "Nightclub" && dropoffLocationType == "Nightclub")
                {
                    return "dynamic_same_type";
                }
                else if (pickupLocationType == dropoffLocationType)
                {
                    return "dynamic_day_same_type";
                }
            }
            return commentType;
        }

        private void ShowCollisionReaction()
        {
            if (currentPassenger == null || !currentPassenger.isPickedUp || currentPassenger.collisionCount == 0)
            {
                return;
            }

            string reaction = currentPassenger.passengerType switch
            {
                PassengerType.Business => currentVehicleCategory switch
                {
                    VehicleCategory.Luxury => "This is unacceptable in a car like this!",
                    VehicleCategory.Economy => "I expected a bumpy ride, but not this!",
                    _ => "Are you serious? That's going to affect your rating!"
                },
                PassengerType.Tourist => currentVehicleCategory switch
                {
                    VehicleCategory.Luxury => "I thought fancy cars were safer!",
                    VehicleCategory.Performance => "Slow down, this isn't a race!",
                    _ => "Oh my! Is this how everyone drives here?"
                },
                PassengerType.Party => currentVehicleCategory switch
                {
                    VehicleCategory.Performance => "Whoa, epic crash! But maybe chill?",
                    _ => "Hey! Easy on the car, I'm not insured for this!"
                },
                PassengerType.Silent => currentVehicleCategory == VehicleCategory.Luxury ? "*glares in disapproval*" : "*gives you a death stare!!*",
                _ => currentVehicleCategory switch
                {
                    VehicleCategory.Luxury => "Careful! This car deserves better!",
                    VehicleCategory.Economy => "I know it’s old, but don’t break it!",
                    _ => "Hey! Careful! This isn't bumper cars!"
                }
            };

            ShowNotification($"{currentPassenger.passengerName}: \"{reaction}\"", NotificationType.Warning, 15f);
            conversationShown = true;
            conversationCount++;
            currentPassenger.negativeInteractions += 2;
            MelonCoroutines.Start(ResetConversationFlag());
        }

        private void HandlePickup(VehicleController playerVehicle, Il2CppNWH.VehiclePhysics2.Damage.DamageHandler damageHandler)
        {
            if (playerVehicle == null) return;
            float distanceToPickup = Vector3.Distance(playerVehicle.transform.position, currentPassenger.pickupLocation);
            bool isAtPickupAddress = IsPlayerAtAddress(currentPassenger.pickupAddress.gameAddress);

            if ((isAtPickupAddress || distanceToPickup < CheckDistance) && playerVehicle.CurrentSpeed < 5f)
            {
                timeAtPickupAddress += Time.deltaTime;
                timeSinceLastOutsideZone = 0f;

                if (timeAtPickupAddress >= PickupConfirmTime)
                {
                    currentPassenger.isPickedUp = true;
                    currentPassenger.pickupTime = Time.time;
                    currentPassenger.collisionCount = 0;
                    currentPassenger.suddenStopCount = 0;
                    currentPassenger.totalSpeedingTime = 0f;
                    lastCollisionTime = Time.time;
                    lastSuddenStopTime = -100f;

                    if (damageHandler != null)
                    {
                        float originalTime = damageHandler.lastCollisionTime;
                        damageHandler.lastCollisionTime = 0f;
                    }

                    ShowNotification("Passenger picked up! Drive to the destination.");

                    if (pickupCircle != null)
                    {
                        UnityEngine.Object.Destroy(pickupCircle);
                        pickupCircle = null;
                    }

                    SpawnDropoffCircle();
                    CityManager.Instance.FindBuildingAndSetGuider(currentPassenger.dropoffAddress.gameAddress, true);
                    timeAtPickupAddress = 0f;
                }
            }
            else
            {
                timeSinceLastOutsideZone += Time.deltaTime;
                if (timeSinceLastOutsideZone > 0.1f)
                {
                    timeAtPickupAddress = 0f;
                }
            }
        }

        private void HandleDropoff(VehicleController playerVehicle)
        {
            if (playerVehicle == null) return;

            float distanceToDropoff = Vector3.Distance(playerVehicle.transform.position, currentPassenger.dropoffLocation);
            bool isAtDropoffAddress = IsPlayerAtAddress(currentPassenger.dropoffAddress.gameAddress);

            if ((isAtDropoffAddress || distanceToDropoff < CheckDistance) && playerVehicle.CurrentSpeed < 5f)
            {
                timeAtPickupAddress += Time.deltaTime;
                timeSinceLastOutsideZone = 0f;

                if (timeAtPickupAddress >= PickupConfirmTime)
                {
                    CompleteUberJob();
                    timeAtPickupAddress = 0f;
                }
            }
            else
            {
                timeSinceLastOutsideZone += Time.deltaTime;
                if (timeSinceLastOutsideZone > 0.1f)
                {
                    timeAtPickupAddress = 0f;
                }
            }
        }

        private void CompleteUberJob()
        {
            isCompletingJob = true;

            UpdateDriverStats();
            float tipAmount = CalculateTipAmount(currentPassenger);
            bool receivedTip = tipAmount > 0f;

            AddMoneyToPlayer(currentPassenger.fare);
            if (receivedTip)
            {
                AddMoneyToPlayer(tipAmount);
            }

            string completionMessage =
            $"Uber job completed!\n\n" +
            $"You earned <color=yellow>${currentPassenger.fare:F2}</color> fare" +
            (receivedTip ? $" + <color=yellow>${tipAmount:F2}</color> tip!" : "") + "\n" +
            $"<color=yellow>{currentPassenger.passengerName}</color> has arrived at " +
            $"<color=white>{currentPassenger.dropoffAddress.DisplayName}</color> " +
            $"<color=yellow>({currentPassenger.dropoffAddress.neighborhood})</color>.";

            ShowNotification(completionMessage);

            hadRegularConversation = false;
            conversationCount = 0;
            CleanupCircles();

            ratingPending = true;
            var completedPassenger = currentPassenger;
            currentPassenger = null;
            uberJobActive = false;
            lastJobCompletionTime = Time.time;

            isCollision = false;
            lastCollisionTime = 0f;
            lastSuddenStopTime = -100f;
            suddenStopCooldown = 0f;
            isSpeeding = false;

            MelonCoroutines.Start(RequestRatingAfterDelay(3f, completedPassenger));
            showUberUI = true;
            uiManager.SetUIVisible(showUberUI);

            if (!uiManager.EnsureContentPanelActive())
            {
                MelonLogger.Error("Failed to ensure UberContentPanel is active after CompleteUberJob!");
            }

            SaveDriverStats();

            isCompletingJob = false;
        }

        private void UpdateDriverStats()
        {
            driverStats.totalRides++;
            driverStats.totalEarnings += currentPassenger.fare;

            if (currentPassenger.distanceToDestination > driverStats.longestRide)
            {
                driverStats.longestRide = currentPassenger.distanceToDestination;
            }

            string neighborhood = currentPassenger.dropoffAddress.neighborhood;

            if (!driverStats.neighborhoodVisits.ContainsKey(neighborhood))
            {
                driverStats.neighborhoodVisits[neighborhood] = 0;
            }

            driverStats.neighborhoodVisits[neighborhood]++;
            var mostVisited = driverStats.neighborhoodVisits.OrderByDescending(x => x.Value).FirstOrDefault();
            driverStats.mostVisitedNeighborhood = mostVisited.Key;
        }

        private float CalculateTipAmount(UberPassenger passenger)
        {
            if (passenger == null || passenger.tipCalculated)
            {
                return 0f;
            }

            float baseTipRate = passengerTypeTipRates.GetValueOrDefault(passenger.passengerType, baselineTipRate);
            string dropoffNeighborhood = passenger.dropoffAddress.neighborhood;

            if (neighborhoodTipRates.ContainsKey(dropoffNeighborhood))
            {
                baseTipRate = (baseTipRate + neighborhoodTipRates[dropoffNeighborhood]) / 2f;
            }

            float tipChance = baseTipRate;
            List<string> tipFactors = new List<string>();

            tipChance = AdjustTipChanceForDriving(tipChance, passenger, tipFactors);
            tipChance = AdjustTipChanceForInteractions(tipChance, passenger, tipFactors);
            tipChance = AdjustTipChanceForTiming(tipChance, passenger, tipFactors);
            tipChance = AdjustTipChanceForVehicle(tipChance, passenger, tipFactors);

            bool givesTip = UnityEngine.Random.Range(0f, 1f) < Mathf.Clamp(tipChance, 0.05f, 0.95f);

            if (!givesTip)
            {
                passenger.tipCalculated = true;
                return 0f;
            }

            float baseTipPercentage = CalculateBaseTipPercentage(passenger);
            float tipAmount = passenger.fare * baseTipPercentage;
            tipAmount = Mathf.Round(tipAmount * 2) / 2;
            tipAmount = Mathf.Max(tipAmount, 1f);

            UpdateDriverTipStats(tipAmount, passenger, baseTipPercentage);

            passenger.tipAmount = tipAmount;
            passenger.tipCalculated = true;
            return tipAmount;
        }

        private float AdjustTipChanceForDriving(float tipChance, UberPassenger passenger, List<string> tipFactors)
        {
            if (passenger.collisionCount == 0 && passenger.suddenStopCount <= 1 && passenger.totalSpeedingTime < 15f)
            {
                tipChance *= tipMultiplierForGoodDriving;
                tipFactors.Add("smooth driving");
            }
            else if (passenger.collisionCount >= 2 || passenger.suddenStopCount >= 3 || passenger.totalSpeedingTime > 60f)
            {
                tipChance *= tipMultiplierForBadDriving;
            }

            return tipChance;
        }

        private float AdjustTipChanceForInteractions(float tipChance, UberPassenger passenger, List<string> tipFactors)
        {
            float conversationFactor = 1.0f;

            if (passenger.positiveInteractions > 0)
            {
                conversationFactor += 0.1f * passenger.positiveInteractions;

                if (passenger.positiveInteractions >= 2)
                {
                    tipFactors.Add("good conversation");
                }
            }

            if (passenger.negativeInteractions > 0)
            {
                conversationFactor -= 0.15f * passenger.negativeInteractions;
            }

            return tipChance * conversationFactor;
        }

        private float AdjustTipChanceForTiming(float tipChance, UberPassenger passenger, List<string> tipFactors)
        {
            if (passenger.waitTime < 60f)
            {
                tipChance *= 1.2f;
                tipFactors.Add("prompt pickup");
            }
            else if (passenger.waitTime > 180f)
            {
                tipChance *= 0.7f;
            }

            if (passenger.dropoffAddress.businessType == BusinessTypeName.JewelryStore || passenger.dropoffAddress.businessType == BusinessTypeName.ElectronicsStore)
            {
                tipChance *= 1.3f;
            }

            if (passenger.passengerType == PassengerType.Business && Time.time % 10 < 3 && passenger.collisionCount == 0)
            {
                tipChance *= 1.5f;
                tipFactors.Add("timely arrival");
            }
            else if (passenger.passengerType == PassengerType.Tourist && passenger.hadSpecialConversation)
            {
                tipChance *= 1.4f;
                tipFactors.Add("local knowledge");
            }

            int currentHour = Il2Cpp.TimeHelper.CurrentHour;

            if (currentHour >= 22 || currentHour <= 4)
            {
                tipChance *= 1.3f;
                tipFactors.Add("late night service");
            }

            if (isPeakHours)
            {
                tipChance *= 0.9f;
            }

            return tipChance;
        }

        private float AdjustTipChanceForVehicle(float tipChance, UberPassenger passenger, List<string> tipFactors)
        {
            float vehicleTipModifier = currentVehicleCategory switch
            {
                VehicleCategory.Luxury => 1.4f,
                VehicleCategory.Performance => 1.2f,
                VehicleCategory.Economy => 0.8f,
                _ => 1f
            };

            float effectiveEnginePower = enginePower ?? 45f;

            if (autoParkSupported && passenger.suddenStopCount < 2)
            {
                vehicleTipModifier *= 1.1f;
                tipFactors.Add("smooth parking");
            }

            if (effectiveEnginePower < 40f)
            {
                vehicleTipModifier *= 0.9f;
                tipFactors.Add("sluggish ride");
            }
            else if (effectiveEnginePower > 80f)
            {
                vehicleTipModifier *= 1.1f;
                tipFactors.Add("powerful ride");
            }

            if (passenger.suddenStopCount > 0)
            {
                var brakes = GetPlayerVehicle()?.gameObject?.GetComponent<Il2CppNWH.VehiclePhysics2.Brakes>();

                if (brakes != null && brakes._brakeInput >= 0.6f)
                {
                    vehicleTipModifier *= 0.95f;
                    tipFactors.Add("hard braking");
                }
            }

            if (vehicleTipModifier > 1f)
            {
                tipFactors.Add("nice car");
            }
            else if (vehicleTipModifier < 1f)
            {
                tipFactors.Add("budget ride");
            }

            return tipChance * vehicleTipModifier;
        }

        private float CalculateBaseTipPercentage(UberPassenger passenger)
        {
            float conversationFactor = 1.0f;

            if (passenger.positiveInteractions > 0)
            {
                conversationFactor += 0.1f * passenger.positiveInteractions;
            }

            if (passenger.negativeInteractions > 0)
            {
                conversationFactor -= 0.15f * passenger.negativeInteractions;
            }

            float baseTipPercentage = UnityEngine.Random.Range(0.1f, 0.3f) * conversationFactor;

            baseTipPercentage *= currentVehicleCategory switch
            {
                VehicleCategory.Luxury => 1.3f,
                VehicleCategory.Performance => 1.1f,
                VehicleCategory.Economy => 0.9f,
                _ => 1f
            };

            if (autoParkSupported)
            {
                baseTipPercentage += 0.02f;
            }

            if (passenger.collisionCount == 0 && passenger.suddenStopCount == 0)
            {
                baseTipPercentage += 0.05f;
            }

            if (UnityEngine.Random.Range(0, 100) < 5)
            {
                baseTipPercentage = UnityEngine.Random.Range(0.4f, 0.5f);
            }

            return Mathf.Clamp(baseTipPercentage, 0.05f, 0.5f);
        }

        private void UpdateDriverTipStats(float tipAmount, UberPassenger passenger, float baseTipPercentage)
        {
            driverStats.totalTipsEarned += tipAmount;
            driverStats.ridesWithTips++;
            driverStats.averageTipPercentage = ((driverStats.averageTipPercentage * (driverStats.ridesWithTips - 1)) + baseTipPercentage) / driverStats.ridesWithTips;

            if (tipAmount > driverStats.highestTip)
            {
                driverStats.highestTip = tipAmount;
            }

            if (!driverStats.tipsByPassengerType.ContainsKey(passenger.passengerType))
            {
                driverStats.tipsByPassengerType[passenger.passengerType] = 0f;
            }

            driverStats.tipsByPassengerType[passenger.passengerType] += tipAmount;
        }

        private void PassengerCancelRide(string reason)
        {
            if (currentPassenger != null)
            {
                driverStats.cancelledRides++;
                ShowNotification(reason, NotificationType.Error);
                uberJobActive = false;
                CleanupCircles();
                currentPassenger = null;
                MelonCoroutines.Start(HideUIAfterDelay(5f));
                SaveDriverStats();
            }
        }
        #endregion

        #region Utility Methods
        private Vector3? GetPositionForAddress(Il2Cpp.Address address)
        {
            if (CityManager.Instance.CityBuildingControllersDictionary.TryGetValue(address, out CityBuildingController buildingController) && buildingController != null)
            {
                if (buildingController.entranceDoors != null && buildingController.entranceDoors.Length > 0)
                {
                    BuildingEntranceDoor entranceDoor = buildingController.entranceDoors[0];
                    if (entranceDoor != null && entranceDoor.doorTransform != null)
                    {
                        Vector3 position = entranceDoor.doorTransform.position;
                        return position;
                    }
                }
                else
                {
                    MelonLogger.Warning($"No entrance doors found for address: {address}");
                }
                Vector3 buildingPos = buildingController.transform.position;
                return buildingPos;
            }
            MelonLogger.Warning($"No BuildingController found for address: {address}");
            return null;
        }

        private GameObject CreateCircleOutline(Vector3 position, float radius, float thickness, Color color)
        {
            GameObject circle = new GameObject("MissionCircleOutline");
            MeshFilter meshFilter = circle.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = circle.AddComponent<MeshRenderer>();

            Mesh mesh = new Mesh();
            const int segments = 128;
            float innerRadius = radius - thickness;

            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uvs = new List<Vector2>();

            for (int i = 0; i <= segments; i++)
            {
                float angle = (float)i / segments * 2f * Mathf.PI;
                float cosAngle = Mathf.Cos(angle);
                float sinAngle = Mathf.Sin(angle);

                float outerX = cosAngle * radius;
                float outerY = sinAngle * radius;
                float innerX = cosAngle * innerRadius;
                float innerY = sinAngle * innerRadius;

                vertices.Add(new Vector3(outerX, outerY, 0f));
                vertices.Add(new Vector3(innerX, innerY, 0f));

                float uvAngle = (float)i / segments;
                uvs.Add(new Vector2(uvAngle, 1f));
                uvs.Add(new Vector2(uvAngle, 0f));
            }

            for (int i = 0; i < segments; i++)
            {
                int outerLeft = i * 2;
                int innerLeft = outerLeft + 1;
                int outerRight = ((i + 1) % segments) * 2;
                int innerRight = outerRight + 1;

                triangles.Add(outerLeft);
                triangles.Add(innerLeft);
                triangles.Add(outerRight);

                triangles.Add(innerLeft);
                triangles.Add(innerRight);
                triangles.Add(outerRight);
            }

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            meshFilter.mesh = mesh;

            Shader shader = FindCircleShader();
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Transparent") ?? Shader.Find("Sprites/Default");
            }

            Material mat = new Material(shader);
            mat.color = new Color(color.r, color.g, color.b, 0.6f);
            mat.SetFloat("_Mode", 3);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3100;

            meshRenderer.material = mat;
            circle.transform.position = position;
            circle.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            circle.transform.localScale = Vector3.one;

            circle.transform.SetParent(null);

            return circle;
        }

        private Shader FindCircleShader()
        {
            if (currentPassenger != null)
            {
                Il2Cpp.Address address = currentPassenger.isPickedUp ? currentPassenger.dropoffAddress.gameAddress : currentPassenger.pickupAddress.gameAddress;

                if (CityManager.Instance.CityBuildingControllersDictionary.TryGetValue(address, out CityBuildingController buildingController) && buildingController != null)
                {
                    if (buildingController.entranceDoors != null && buildingController.entranceDoors.Length > 0)
                    {
                        BuildingEntranceDoor entranceDoor = buildingController.entranceDoors[0];

                        if (entranceDoor != null && entranceDoor.doorTransform != null)
                        {
                            MeshRenderer doorRenderer = entranceDoor.doorTransform.GetComponent<MeshRenderer>();

                            if (doorRenderer != null && doorRenderer.material != null && doorRenderer.material.shader != null)
                            {
                                return doorRenderer.material.shader;
                            }
                        }
                    }
                }
            }

            Shader fallback = Shader.Find("Sprites/Default") ?? Shader.Find("Legacy Shaders/Transparent/Diffuse");
            return fallback;
        }

        private void SpawnPickupCircle()
        {
            Vector3? position = GetPositionForAddress(currentPassenger.pickupAddress.gameAddress);

            if (position.HasValue)
            {
                Vector3 circlePosition = position.Value + new Vector3(0, 0.1f, 0);
                pickupCircle = CreateCircleOutline(circlePosition, 9f, 0.25f, new Color(0, 1, 0, 0.3f));
            }
            else
            {
            }
        }

        private void SpawnDropoffCircle()
        {
            Vector3? position = GetPositionForAddress(currentPassenger.dropoffAddress.gameAddress);

            if (position.HasValue)
            {
                Vector3 circlePosition = position.Value + new Vector3(0, 0.1f, 0);
                dropoffCircle = CreateCircleOutline(circlePosition, 9f, 0.25f, new Color(1, 0, 0, 0.3f));
            }
            else
            {
            }
        }

        private VehicleController GetPlayerVehicle()
        {
            try
            {
                return GameManager.Instance?.selectedVehicle;
            }
            catch
            {
                return null;
            }
        }

        private void ShowNotification(string message, NotificationType notificationType = NotificationType.Info, float displayTime = 5f)
        {
            try
            {
                if (notificationUI == null)
                {
                    MelonLogger.Error("Cannot show notification: notificationUI is null");
                    return;
                }

                if (message.Contains("completed"))
                {
                    notificationType = NotificationType.Success;
                }
                else if (message.Contains("canceled"))
                {
                    notificationType = NotificationType.Error;
                }

                if (message.Contains("Request") || message.Contains("accepted"))
                {
                    displayTime = 15f;
                }

                notificationUI.ShowNotification(message, notificationType, displayTime);
            }
            catch (Exception ex)
            {
            }
        }

        private void ShowDrivingNotification(string eventType, NotificationType notificationType, float displayTime)
        {
            try
            {
                if (notificationUI == null || currentPassenger == null)
                {
                    return;
                }

                if (drivingDialogues == null)
                {
                    return;
                }

                var messages = drivingDialogues.NotificationMessages[currentPassenger.passengerType][eventType];
                string message = messages[UnityEngine.Random.Range(0, messages.Count)];
                string firstName = playerName.Split(' ')[0];
                message = message.Replace("{playerName}", firstName);
                notificationUI.ShowNotification(message, notificationType, displayTime);
            }
            catch (Exception ex)
            {
            }
        }

        private void AddMoneyToPlayer(float amount)
        {
            try
            {
                var transaction = new Il2Cpp.Transaction();
                Il2Cpp.Transaction.TransactionType transactionType = Il2Cpp.Transaction.TransactionType.TaxiRide;
                var dataHolder = new Il2Cpp.Transaction.DataHolder { value = amount };
                int currentDay = Il2Cpp.TimeHelper.CurrentDay;
                var nullableDay = new Il2CppSystem.Nullable<int>(currentDay);

                GameManager.ChangeMoneySafe(
                    amount,
                    transactionType,
                    dataHolder,
                    nullableDay,
                    currentPassenger?.dropoffAddress.gameAddress,
                    true
                );
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error adding money to player: {ex.Message}");
            }
        }

        private float CalculateFare(float distance)
        {
            const float baseFare = 5.0f;
            const float minutePerMeter = 0.04f;
            const float pricePerMeter = 0.2f;
            const float pricePerMinute = 0.07f;

            float estimatedTime = distance * minutePerMeter;
            float randomFactor = UnityEngine.Random.Range(0.95f, 1.05f);

            float passengerModifier = currentPassenger != null ? currentPassenger.passengerType switch
            {
                PassengerType.Business => 1.2f,
                PassengerType.Tourist => 1.1f,
                PassengerType.Party => 0.95f,
                PassengerType.Silent => 0.9f,
                _ => 1f
            } : 1f;

            float vehicleModifier = currentVehicleCategory switch
            {
                VehicleCategory.Luxury => 1.3f,
                VehicleCategory.Performance => 1.2f,
                VehicleCategory.Economy => 0.85f,
                _ => 1f
            };

            int currentHour = Il2Cpp.TimeHelper.CurrentHour;
            float timeModifier = (currentHour >= 22 || currentHour <= 4) ? 1.15f : 1f;

            float fare = (baseFare + (pricePerMeter * distance) + (pricePerMinute * estimatedTime)) *
                         randomFactor * passengerModifier * vehicleModifier * timeModifier;

            if (isPeakHours)
            {
                fare *= PeakMultiplier;
            }

            return Mathf.Max(fare, 2f);
        }

        private bool IsPlayerAtAddress(Il2Cpp.Address targetAddress)
        {
            return lastClosestBuildingAddress != null && lastClosestBuildingAddress.Equals(targetAddress);
        }

        private void UpdateNeighborhood()
        {
            if (playerHUD == null)
            {
                currentNeighborhood = "Unknown";
                return;
            }

            if (playerHUD._lastClosestBuilding == null || playerHUD._lastClosestBuilding.Equals(null))
            {
                currentNeighborhood = "Unknown";
                return;
            }

            try
            {
                lastClosestBuildingAddress = playerHUD._lastClosestBuilding.Address;
                currentNeighborhood = playerHUD._lastClosestBuilding.Neighbourhood.ToString();
            }
            catch (Exception ex)
            {
                currentNeighborhood = "Unknown";
            }
        }

        private void UpdateDrivingState(VehicleController playerVehicle)
        {
            if (playerVehicle == null || isCompletingJob)
            {
                ResetVehicleState();
                return;
            }

            float currentTime = Time.time;
            currentSpeed = playerVehicle.CurrentSpeed;

            var damageHandler = playerVehicle.gameObject?.GetComponent<Il2CppNWH.VehiclePhysics2.Damage.DamageHandler>();
            var speedLimiter = playerVehicle.gameObject?.GetComponent<Il2CppNWH.VehiclePhysics2.Modules.SpeedLimiter.SpeedLimiterModule>();

            UpdateVehicleType(playerVehicle);
            UpdateDrivingMetrics(playerVehicle, currentTime, damageHandler, speedLimiter);
            UpdateSmoothDriving(currentTime);
            previousSpeed = currentSpeed;
            previousTime = currentTime;
        }

        private void ResetVehicleState()
        {
            if (lastDetectedVehicle != null)
            {
                lastDetectedVehicle = null;
                lastVehicleTypeName = null;
                maxVehicleSpeed = null;
                vehiclePrice = null;
                enginePower = null;
                autoParkSupported = false;
                currentVehicleCategory = VehicleCategory.Standard;
                isCheapCar = false;
            }
        }

        private void UpdateVehicleType(VehicleController playerVehicle)
        {
            if (playerVehicle != lastDetectedVehicle || lastVehicleTypeName == null)
            {
                try
                {
                    var vehicleType = GetVehicleType(playerVehicle);

                    if (vehicleType != null)
                    {
                        string currentVehicleTypeName = vehicleType.vehicleTypeName.ToString();

                        if (currentVehicleTypeName != lastVehicleTypeName)
                        {
                            maxVehicleSpeed = Mathf.Min(vehicleType.maxSpeed, 100f);
                            vehiclePrice = vehicleType.price;
                            enginePower = vehicleType.enginePower;
                            autoParkSupported = vehicleType.autoParkSupported;
                            currentVehicleCategory = CategorizeVehicle(vehicleType);
                            isCheapCar = currentVehicleCategory == VehicleCategory.Economy;
                            lastDetectedVehicle = playerVehicle;
                            lastVehicleTypeName = currentVehicleTypeName;
                        }
                    }
                    else
                    {
                        ResetVehicleState();
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Error detecting vehicle type: {ex.Message}");
                    ResetVehicleState();
                }
            }
        }

        private void UpdateDrivingMetrics(VehicleController playerVehicle, float currentTime, Il2CppNWH.VehiclePhysics2.Damage.DamageHandler damageHandler, Il2CppNWH.VehiclePhysics2.Modules.SpeedLimiter.SpeedLimiterModule speedLimiter)
        {
            float effectiveMaxSpeed = maxVehicleSpeed ?? 100f;
            float effectiveEnginePower = enginePower ?? 45f;

            float adjustedAccelerationThreshold = AccelerationThreshold * (effectiveEnginePower < 40f ? 1.2f : effectiveEnginePower > 80f ? 0.8f : 1f);
            dynamicSpeedLimit = CalculateDynamicSpeedLimit(maxVehicleSpeed, currentVehicleCategory);

            float adjustedSpeedingTolerance = isCheapCar ? 20f : currentVehicleCategory == VehicleCategory.Performance ? 15f : 10f;

            if (lastVehicleTypeName != null && lastVehicleTypeName.Contains("Honza Mimic"))
            {
                adjustedSpeedingTolerance = 25f;
            }

            if (speedLimiter != null)
            {
                hasSpeedLimiterModule = true;
                gameSpeedLimit = speedLimiter.speedLimit;
                dynamicSpeedLimit = Mathf.Min(dynamicSpeedLimit, gameSpeedLimit);
            }
            else
            {
                hasSpeedLimiterModule = false;
                gameSpeedLimit = float.MaxValue;
            }

            HandleCollisions(playerVehicle, damageHandler, currentTime);
            HandleSuddenStops(playerVehicle, currentTime, adjustedAccelerationThreshold);
            HandleSpeeding(currentTime, adjustedSpeedingTolerance);
        }

        private void HandleCollisions(VehicleController playerVehicle, Il2CppNWH.VehiclePhysics2.Damage.DamageHandler damageHandler, float currentTime)
        {
            if (damageHandler != null && playerVehicle != null)
            {
                float vehicleLastCollisionTime = damageHandler.lastCollisionTime;

                bool isDuringPickup = currentPassenger != null && !currentPassenger.isPickedUp && timeAtPickupAddress > 0f;

                if (isDuringPickup)
                {
                    return;
                }

                if (currentPassenger != null && vehicleLastCollisionTime < currentPassenger.pickupTime)
                {
                    return;
                }

                if (sessionStartTime == 0f && currentPassenger != null)
                {
                    sessionStartTime = currentPassenger.pickupTime;
                }

                if (sessionStartTime > 0f && vehicleLastCollisionTime < sessionStartTime)
                {
                    return;
                }

                if (vehicleLastCollisionTime > lastCollisionDebounceTime + 1.5f)
                {
                    if (currentPassenger != null &&
                        currentPassenger.isPickedUp &&
                        vehicleLastCollisionTime > currentPassenger.pickupTime)
                    {
                        if (currentSpeed < 2f)
                        {
                            return;
                        }

                        bool isNearPickupOrDropoff = IsNearPickupOrDropoff(playerVehicle);
                        if (isNearPickupOrDropoff)
                        {
                            return;
                        }

                        lastCollisionTime = vehicleLastCollisionTime;
                        lastCollisionDebounceTime = vehicleLastCollisionTime;
                        isCollision = true;
                        currentPassenger.collisionCount++;

                        smoothDrivingTime = Mathf.Max(0, smoothDrivingTime - 60f);

                        float collisionPenalty = currentVehicleCategory == VehicleCategory.Luxury ? 1.5f :
                                                currentVehicleCategory == VehicleCategory.Performance ? 2f : 1f;
                        if (currentSpeed < 10f)
                        {
                            collisionPenalty *= 0.5f;
                        }
                        currentPassenger.negativeInteractions += (int)collisionPenalty;

                        if (currentPassenger.passengerType != PassengerType.Silent || UnityEngine.Random.Range(0, 100) < 80)
                        {
                            ShowCollisionReaction();
                        }
                    }
                }
                else if (isCollision && (currentTime - lastCollisionTime) > CollisionRecoveryTime)
                {
                    isCollision = false;
                }
            }
        }

        private bool IsNearPickupOrDropoff(VehicleController playerVehicle)
        {
            if (currentPassenger == null || playerVehicle == null)
            {
                return false;
            }

            float distanceToPickup = Vector3.Distance(playerVehicle.transform.position, currentPassenger.pickupLocation);
            float distanceToDropoff = currentPassenger.isPickedUp ? Vector3.Distance(playerVehicle.transform.position, currentPassenger.dropoffLocation) : float.MaxValue;
            bool isNear = distanceToPickup < CheckDistance || distanceToDropoff < CheckDistance;
            return isNear;
        }

        private void HandleSuddenStops(VehicleController playerVehicle, float currentTime, float adjustedAccelerationThreshold)
        {
            bool isSuddenStop = false;
            float deltaTime = Mathf.Min(currentTime - previousTime, 0.1f);
            float acceleration = (currentSpeed - previousSpeed) / deltaTime;

            if (previousSpeed > 10f && acceleration < -adjustedAccelerationThreshold && (previousSpeed - currentSpeed) > 5f)
            {
                if (!currentPassenger.isPickedUp)
                {
                    return;
                }

                if (playerVehicle == null)
                {
                    return;
                }

                bool isNearPickupOrDropoff = IsNearPickupOrDropoff(playerVehicle);
                if (!isNearPickupOrDropoff)
                {
                    isSuddenStop = true;
                    if (currentTime - lastSuddenStopTime >= suddenStopCooldown)
                    {
                        lastSuddenStopTime = currentTime;
                        suddenStopCooldown = SuddenStopCooldownDuration;
                        currentPassenger.suddenStopCount++;
                        float stopPenalty = currentVehicleCategory == VehicleCategory.Luxury ? 1.5f : 1f;
                        smoothDrivingTime = Mathf.Max(0, smoothDrivingTime - 20f * stopPenalty);
                        ShowDrivingNotification("sudden_stop", NotificationType.Warning, 3f);
                    }
                }
            }

            if (suddenStopCooldown > 0)
            {
                suddenStopCooldown = Mathf.Max(0, suddenStopCooldown - (currentTime - previousTime));
            }
        }

        private void HandleSpeeding(float currentTime, float adjustedSpeedingTolerance)
        {
            float effectiveSpeedLimit = dynamicSpeedLimit;
            float speedThreshold = effectiveSpeedLimit * (1f + adjustedSpeedingTolerance / 100f);

            if (currentSpeed > speedThreshold)
            {
                float deltaTime = Mathf.Min(currentTime - previousTime, 0.1f);
                speedingTimer += deltaTime;

                if (!wasSpeeding && speedingTimer > 0.1f)
                {
                    ShowDrivingNotification("speeding", NotificationType.Warning, 3f);
                    wasSpeeding = true;
                }

                if (speedingTimer >= 520f)
                {
                    float speedingFactor = currentVehicleCategory == VehicleCategory.Performance ? 0.5f : 1f;
                    currentPassenger.totalSpeedingTime += deltaTime * speedingFactor;

                    if (currentPassenger.totalSpeedingTime > 5f && currentTime - lastSpeedingNotificationTime >= 15f)
                    {
                        lastSpeedingNotificationTime = currentTime;
                        ShowDrivingNotification("speeding", NotificationType.Warning, 3f);
                    }
                }
            }
            else
            {
                speedingTimer = 0f;
                wasSpeeding = false;
                float deltaTime = Mathf.Min(currentTime - previousTime, 0.1f);
                currentPassenger.totalSpeedingTime = Mathf.Max(0, currentPassenger.totalSpeedingTime - (deltaTime * 0.5f));
            }
        }

        private void UpdateSmoothDriving(float currentTime)
        {
            float deltaTime = Mathf.Min(currentTime - previousTime, 0.1f);
            float acceleration = previousTime > 0 ? (currentSpeed - previousSpeed) / deltaTime : 0f;

            if (currentSpeed > 15f && Mathf.Abs(acceleration) < SmoothAccelerationThreshold && !isCollision && !isSpeeding)
            {
                smoothDrivingTime += deltaTime;
            }
            else if (currentSpeed < 5f)
            {
                smoothDrivingTime = Mathf.Max(0, smoothDrivingTime - (deltaTime * 0.2f));
            }
        }

        private VehicleType GetVehicleType(VehicleController vehicle)
        {
            if (vehicle == null || vehicle.gameObject == null || vehicle.vehicleType == null)
            {
                return null;
            }

            try
            {
                VehicleTypeName vehicleTypeName = vehicle.vehicleType.vehicleTypeName;
                var vehicleTypes = Il2CppVehicles.VehicleTypes.VehicleTypeHelper.VehicleTypes;

                if (vehicleTypes == null)
                {
                    return null;
                }

                foreach (var pair in vehicleTypes)
                {
                    if (pair.Key == vehicleTypeName)
                    {
                        return pair.Value;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error getting vehicle type: {ex.Message}");
                return null;
            }
        }

        private VehicleCategory CategorizeVehicle(VehicleType vehicleType)
        {
            if (vehicleType == null)
            {
                return VehicleCategory.Standard;
            }

            float price = vehicleType.price;
            float maxSpeed = vehicleType.maxSpeed;
            float enginePower = vehicleType.enginePower;
            bool autoParkSupported = vehicleType.autoParkSupported;

            if (price > 200000f || (enginePower > 80f && autoParkSupported))
            {
                return VehicleCategory.Luxury;
            }
            else if (maxSpeed > 80f || enginePower > 60f)
            {
                return VehicleCategory.Performance;
            }
            else if (price < 50000f || enginePower < 40f)
            {
                return VehicleCategory.Economy;
            }

            return VehicleCategory.Standard;
        }

        private float CalculateDynamicSpeedLimit(float? maxVehicleSpeed, VehicleCategory vehicleCategory)
        {
            float baseSpeedLimit = 18f;

            if (maxVehicleSpeed.HasValue)
            {
                baseSpeedLimit = Mathf.Min(maxVehicleSpeed.Value * 0.22f, 22f);
                if (maxVehicleSpeed.Value <= 60f)
                {
                    baseSpeedLimit = Mathf.Max(baseSpeedLimit, 12f);
                }
            }

            float categoryModifier = vehicleCategory switch
            {
                VehicleCategory.Luxury => 0.9f,
                VehicleCategory.Performance => 1.2f,
                VehicleCategory.Economy => 0.85f,
                _ => 1f
            };

            float neighborhoodModifier = currentNeighborhood switch
            {
                "Midtown" => 0.85f,
                "LowerManhattan" => 0.95f,
                "HellsKitchen" => 1f,
                _ => 1f
            };

            return baseSpeedLimit * categoryModifier * neighborhoodModifier;
        }

        private void CleanupCircles()
        {
            if (!uiManager.EnsureContentPanelActive())
            {
                MelonLogger.Error("UberContentPanel unavailable in CleanupCircles!");
            }
            if (pickupCircle != null)
            {
                UnityEngine.Object.Destroy(pickupCircle);
                pickupCircle = null;
            }

            if (dropoffCircle != null)
            {
                UnityEngine.Object.Destroy(dropoffCircle);
                dropoffCircle = null;
            }
        }

        private void SaveDriverStats()
        {
            try
            {
                string saveDir = Path.Combine(MelonEnvironment.UserDataDirectory, "UberJobMod");
                string savePath = Path.Combine(saveDir, "DriverStats.json");

                if (!Directory.Exists(saveDir))
                {
                    Directory.CreateDirectory(saveDir);
                }

                string json = JsonConvert.SerializeObject(driverStats, Formatting.Indented);
                File.WriteAllText(savePath, json);
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error saving driver stats: {ex.Message}");
            }
        }
        #endregion
    }
}