using MelonLoader;
using MelonLoader.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Il2Cpp;
using Il2CppUI.PlayerHUD;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using UberSideJobMod.UberSideJobMod;
using Il2CppEnums;
using Il2CppVehicles.VehicleTypes;
using Il2CppCharacter;
using Il2CppBigAmbitions.Characters;
using Il2CppUI.Smartphone.Apps.Persona;
using Harmony;
namespace UberSideJobMod
{
    public class UberJobMod : MelonMod
    {
        #region Fields and Constants

        // Vehicle
        private VehicleCategory currentVehicleCategory = VehicleCategory.Standard;
        private float? maxVehicleSpeed = null; // Null until vehicle detected
        private float? vehiclePrice = null;
        private float? enginePower = null;
        private bool autoParkSupported = false; // Default false for cheap cars
        private VehicleController lastDetectedVehicle = null;
        private string lastVehicleTypeName = null;

        // Core state
        private List<Address> addresses = new List<Address>();
        private UberPassenger currentPassenger = null;
        private bool uberJobActive = false;
        private bool showUberUI = false;
        private DriverStats driverStats = new DriverStats();
        private UberNotificationUI notificationUI;

        // Timing and intervals
        private float lastPassengerOfferTime = 0f;
        private float lastJobCompletionTime = -UberJobCooldown;
        private float lastConversationTime = 0f;
        private float lastCollisionTime = 0f;
        private float lastSuddenStopTime = -100f;
        private float smoothDrivingTime = 0f;

        // Game objects and references
        private Dictionary<Il2Cpp.Address, Vector3> buildingPositions = new Dictionary<Il2Cpp.Address, Vector3>();
        private PlayerHUD playerHUD = null;
        private Il2Cpp.Address lastClosestBuildingAddress = null;
        private string currentNeighborhood = "Unknown";
        private UberUIManager uiManager;

        // Driving state tracking
        private float previousSpeed = 0f;
        private float previousTime = 0f;
        private float currentSpeed = 0f; // Tracks latest vehicle speed
        private bool isSpeeding = false;
        private bool isCollision = false;
        private bool hasSpeedLimiterModule = false;
        private float gameSpeedLimit = float.MaxValue; // Default to "no limiter" until updated
        private float dynamicSpeedLimit = 50f; // Base speed limit, updated per vehicle
        private float speedingTolerance = 5f; // Percentage above dynamicSpeedLimit (e.g., 10%)
        private bool isCheapCar = false; // Low-performance vehicle flag
        private float suddenStopCooldown = 0f; // Cooldown timer for Sudden Stop triggers
        private const float SuddenStopCooldownDuration = 10f;
        private float lastSpeedingNotificationTime = -100f; // Track last Speeding notification
        private const float SpeedingNotificationCooldown = 25f; // 15s cooldown for Speeding notifications

        // UI and conversation state
        private bool conversationShown = false;
        private bool ratingPending = false;
        private bool hadRegularConversation = false;
        private int conversationCount = 0;
        private bool isPeakHours = false;

        // Constants
        private const float CheckDistance = 15f;
        private const float PassengerOfferInterval = 30f;
        private const float UberJobCooldown = 120f;
        private const float MaxWaitingTime = 300f;
        private const float ConversationInterval = 20f;
        private const float CollisionRecoveryTime = 8f;
        private const float AccelerationThreshold = 10f;   // Units/second for sudden stops
        private const float SmoothAccelerationThreshold = 15f; // Units/second for smooth driving
        private const float PeakMultiplier = 1.5f;

        // Tips System
        private float baselineTipRate = 0.15f;
        private float tipMultiplierForGoodDriving = 1.5f;
        private float tipMultiplierForBadDriving = 0.5f;
        private Dictionary<PassengerType, float> passengerTypeTipRates = new Dictionary<PassengerType, float>();
        private Dictionary<string, float> neighborhoodTipRates = new Dictionary<string, float>();

        // Player State
        private Gender playerGender;
        private CharacterData playerCharacterData;
        private string playerName;

        private readonly Dictionary<PassengerType, Dictionary<string, List<string>>> notificationMessages = new Dictionary<PassengerType, Dictionary<string, List<string>>>
        {
            [PassengerType.Business] = new Dictionary<string, List<string>>
            {
                ["speeding"] = new List<string>
               {
                   "{playerName}, I’m on a tight schedule—can you slow down?",
                   "{playerName}, this speed is unprofessional, please ease up!",
                   "{playerName}, slow down, I’d like to arrive in one piece!"
               },
                ["sudden_stop"] = new List<string>
               {
                   "{playerName}, that stop was abrupt—fortunately, I’m belted in!",
                   "{playerName}, please, no more sudden stops, I have a meeting!",
                   "{playerName}, a smoother ride would be much appreciated!"
               }
            },
            [PassengerType.Tourist] = new Dictionary<string, List<string>>
            {
                ["speeding"] = new List<string>
               {
                   "{playerName}, whoa, slow down—I want to enjoy the sights!",
                   "{playerName}, can you ease up? I’m here to explore, not race!",
                   "{playerName}, slow down, I’d love to take some photos!"
               },
                ["sudden_stop"] = new List<string>
               {
                   "{playerName}, that stop startled me—good thing I’m buckled up!",
                   "{playerName}, oh my, let’s avoid those sudden stops, okay?",
                   "{playerName}, that was a jolt—let’s keep it smooth, please!"
               }
            },
            [PassengerType.Party] = new Dictionary<string, List<string>>
            {
                ["speeding"] = new List<string>
               {
                   "{playerName}, yo, slow down—we’re not at the club yet!",
                   "{playerName}, chill on the speed, let’s keep the vibes cool!",
                   "{playerName}, ease up, I don’t wanna spill my drink!"
               },
                ["sudden_stop"] = new List<string>
               {
                   "{playerName}, whoa, that stop—luckily I’m strapped in!",
                   "{playerName}, easy on the brakes, let’s keep the party going!",
                   "{playerName}, that stop killed the vibe—smoother, please!"
               }
            },
            [PassengerType.Silent] = new Dictionary<string, List<string>>
            {
                ["speeding"] = new List<string>
               {
                   "{playerName}, *sighs* Can you slow down?",
                   "{playerName}, *frowns* Please, less speed.",
                   "{playerName}, *quietly* Slow down, okay?"
               },
                ["sudden_stop"] = new List<string>
               {
                   "{playerName}, *grips seat* I’m belted, but still…",
                   "{playerName}, *mutters* That stop was harsh.",
                   "{playerName}, *silent glare* Smoother, please."
               }
            },
            [PassengerType.Regular] = new Dictionary<string, List<string>>
            {
                ["speeding"] = new List<string>
               {
                   "{playerName}, can you slow down a bit, please?",
                   "{playerName}, let’s take it easy on the speed, okay?",
                   "{playerName}, slow down, I’m not in a rush!"
               },
                ["sudden_stop"] = new List<string>
               {
                   "{playerName}, that stop was rough—good thing I’m buckled!",
                   "{playerName}, let’s keep it smooth, that stop was jarring!",
                   "{playerName}, whoa, easy on the brakes, please!"
               }
            }
        };

        #endregion

        #region Initialization Methods

        public override void OnInitializeMelon()
        {
            MelonLogger.Msg("Uber Mod Big Ambitions - loaded successfully!");
            MelonCoroutines.Start(WaitForGameLoad());
            LoadDriverStats();
            InitializeTipSystem();
            uiManager = new UberUIManager();
            uiManager.InitializeUI();
            uiManager.OnAcceptClicked += OnAcceptButtonClicked;
            uiManager.OnDeclineClicked += OnDeclineButtonClicked;
            uiManager.OnCancelClicked += OnCancelButtonClicked;
            uiManager.SetUIVisible(false);
            notificationUI = new UberNotificationUI();
            notificationUI.Initialize();
        }

        private void InitializeTipSystem()
        {
            // Initialize passenger type tip rates
            passengerTypeTipRates[PassengerType.Business] = 0.25f; // Business passengers tip more
            passengerTypeTipRates[PassengerType.Tourist] = 0.20f; // Tourists tip moderately
            passengerTypeTipRates[PassengerType.Party] = 0.15f; // Party people tip average
            passengerTypeTipRates[PassengerType.Silent] = 0.10f; // Silent passengers tip less
            passengerTypeTipRates[PassengerType.Regular] = 0.15f; // Regular passengers tip average

            // Initialize neighborhood tip rates (example values - adjust based on your game's neighborhoods)
            neighborhoodTipRates["Midtown"] = 0.25f;
            neighborhoodTipRates["LowerManhattan"] = 0.15f;
            neighborhoodTipRates["HellsKitchen"] = 0.15f;
            neighborhoodTipRates["GarmentDistrict"] = 0.10f;
            neighborhoodTipRates["MurrayHill"] = 0.20f;

            // Initialize tip stats in driver stats if needed
            if (driverStats.tipsByPassengerType == null)
                driverStats.tipsByPassengerType = new Dictionary<PassengerType, float>();

            foreach (PassengerType type in Enum.GetValues(typeof(PassengerType)))
            {
                if (!driverStats.tipsByPassengerType.ContainsKey(type))
                    driverStats.tipsByPassengerType[type] = 0f;
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

            // Wait for CharacterData via PlayerHelper
            playerCharacterData = null;
            float timeout = 15f; // 15s for Persona app load
            float elapsed = 0f;

            while (playerCharacterData == null && elapsed < timeout)
            {
                try
                {
                    playerCharacterData = Il2CppHelpers.PlayerHelper.GetPlayerCharacterData();
                }
                catch
                {
                    // Silent catch to retry
                }

                if (playerCharacterData == null)
                {
                    yield return new WaitForSeconds(0.5f);
                    elapsed += 0.5f;
                }
            }

            // Set gender and try CharacterData.name
            try
            {
                if (playerCharacterData != null)
                {
                    // Map Il2CppBigAmbitions.Characters.Gender to UberSideJobMod.Gender
                    playerGender = playerCharacterData.gender switch
                    {
                        Il2CppBigAmbitions.Characters.Gender.Male => Gender.Male,
                        Il2CppBigAmbitions.Characters.Gender.Female => Gender.Female,
                        _ => Gender.Male // Default to Male if gender is unknown
                    };
                    playerName = playerCharacterData.name ?? "Driver"; // Use null-coalescing operator for safety
                }
                else
                {
                    playerGender = Gender.Male;
                    playerName = "Driver";
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Failed to retrieve player character data: {ex.Message}");
                playerGender = Gender.Male;
                playerName = "Driver";
            }

            // Fallback to CharacterInfo.characterName if name is null
            if (string.IsNullOrEmpty(playerName))
            {
                CharacterInfo characterInfo = null;
                elapsed = 0f;
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
                    playerName = "Driver"; // Final fallback
                }
            }

            LoadAddressData();

            if (addresses.Count == 0)
            {
                yield return new WaitForSeconds(10f);
                LoadAddressData();
            }

            MelonCoroutines.Start(EfficientUberJobRoutine());
            MelonCoroutines.Start(TimeCheckRoutine());
            MelonCoroutines.Start(DrivingStateRoutine());
        }
        private IEnumerator DrivingStateRoutine()
        {
            while (true)
            {
                if (uberJobActive && currentPassenger != null && currentPassenger.isPickedUp)
                {
                    UpdateDrivingState(GetPlayerVehicle());
                }
                yield return new WaitForSeconds(0.2f); // Check every 0.2s
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
                    Vector3 buildingPos = buildingController.transform.position;

                    // Check for duplicate gameAddress
                    if (buildingPositions.ContainsKey(gameAddress))
                    {
                        continue;
                    }
                    buildingPositions[gameAddress] = buildingPos;

                    // Get BuildingRegistration
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

                            // Whitelist Residential, Warehouse, Special
                            string lowerBuildingType = buildingType.ToLower();
                            if (lowerBuildingType == "residential" || lowerBuildingType == "warehouse" || lowerBuildingType == "special")
                            {
                                businessType = BusinessTypeName.Empty;
                                businessName = "Unknown Business";
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }

                    // Check for duplicate Address entries
                    if (addresses.Any(a => a.gameAddress == gameAddress))
                    {
                        MelonLogger.Warning($"Duplicate Address entry for gameAddress: {gameAddress}");
                        continue;
                    }

                    addresses.Add(new Address
                    {
                        gameAddress = gameAddress,
                        address = businessName, // Legacy
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
                    Directory.CreateDirectory(saveDir);
                if (File.Exists(savePath))
                {
                    string json = File.ReadAllText(savePath);
                    driverStats = JsonConvert.DeserializeObject<DriverStats>(json);
                    MelonLogger.Msg($"Loaded driver stats: {driverStats.totalRides} rides, ${driverStats.totalEarnings:F2} earned");
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
            while (true)
            {
                float waitTime = uberJobActive ? 0.5f : 1f; // Reduced wait for idle checks
                if (showUberUI || uberJobActive)
                {
                    UpdateNeighborhood();
                    VehicleController playerVehicle = GetPlayerVehicle();
                    if (playerVehicle != null && showUberUI && !uberJobActive && currentPassenger == null)
                    {
                        idleTimeWithoutJob += waitTime;
                        if (Time.time - lastJobCompletionTime > UberJobCooldown &&
                            (Time.time - lastPassengerOfferTime > PassengerOfferInterval || idleTimeWithoutJob > 45f))
                        {
                            OfferNewPassenger();
                            lastPassengerOfferTime = Time.time;
                            idleTimeWithoutJob = 0f;
                        }
                    }
                    else
                    {
                        idleTimeWithoutJob = 0f; // Reset if not eligible
                    }

                    if (uberJobActive && currentPassenger != null)
                    {
                        if (currentPassenger.isPickedUp && !conversationShown && Time.time - lastConversationTime > ConversationInterval)
                        {
                            ShowPassengerConversation();
                            lastConversationTime = Time.time;
                        }

                        if (!currentPassenger.isPickedUp)
                        {
                            currentPassenger.waitTime += waitTime;
                            if (currentPassenger.waitTime > MaxWaitingTime && UnityEngine.Random.Range(0, 100) < 5)
                            {
                                PassengerCancelRide("The passenger got tired of waiting and cancelled the ride.");
                            }
                        }

                        if (playerVehicle != null)
                        {
                            CheckDestinationReached(playerVehicle);
                        }
                    }
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
                        MelonLogger.Msg("Entering peak hours - surge pricing active");
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Error checking time: {ex.Message}");
                }
                yield return new WaitForSeconds(60f);
            }
        }

        private IEnumerator ResetConversationFlag()
        {
            yield return new WaitForSeconds(10f);
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
            if (passenger.totalSpeedingTime > 30f && (!isCheapCar || maxVehicleSpeed > dynamicSpeedLimit * 1.3f))
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
            if (passenger.collisionCount > 0) finalRating = Mathf.Min(finalRating, 4.0f);
            if (passenger.collisionCount >= 3) finalRating = Mathf.Min(finalRating, 2.5f);

            string feedbackMessage = feedbackReasons.Count == 1 ? $"Because of {feedbackReasons[0]}." :
                                    feedbackReasons.Count > 1 ? $"Because of {string.Join(", ", feedbackReasons.Take(feedbackReasons.Count - 1))} and {feedbackReasons.Last()}." : "";

            driverStats.ratings.Add(finalRating);
            driverStats.averageRating = driverStats.ratings.Average();

            string notificationMessage = $"{passenger.passengerName} rated you {finalRating:F1} stars!" + (string.IsNullOrEmpty(feedbackMessage) ? "" : $"\n{feedbackMessage}");
            NotificationType notificationType = finalRating >= 4f ? NotificationType.Success : finalRating >= 3f ? NotificationType.Info : NotificationType.Warning;
            ShowNotification(notificationMessage, notificationType);

            ratingPending = false;
            SaveDriverStats();
            lastPassengerOfferTime = Time.time - PassengerOfferInterval + 15f;
        }

        private IEnumerator HideUIAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            showUberUI = false;
        }

        #endregion

        #region Event Handlers

        public override void OnUpdate()
        {
            if (uiManager == null)
            {
                MelonLogger.Warning("uiManager is null in OnUpdate, initializing...");
                uiManager = new UberUIManager();
                uiManager.InitializeUI();
                uiManager.OnAcceptClicked += OnAcceptButtonClicked;
                uiManager.OnDeclineClicked += OnDeclineButtonClicked;
                uiManager.OnCancelClicked += OnCancelButtonClicked;
                uiManager.SetUIVisible(false);
            }

            if (showUberUI)
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
                uiManager.UpdateUI(driverStats, currentPassenger, isPeakHours, uberJobActive);
            }

            if (Input.GetKeyDown(KeyCode.F3))
            {
                showUberUI = !showUberUI;
                uiManager.SetUIVisible(showUberUI);
                if (showUberUI)
                {
                    if (!uberJobActive && currentPassenger == null && Time.time - lastPassengerOfferTime > PassengerOfferInterval && Time.time - lastJobCompletionTime > UberJobCooldown)
                    {
                        OfferNewPassenger();
                        lastPassengerOfferTime = Time.time - PassengerOfferInterval + 15f;
                    }
                }
                else if (!uberJobActive && currentPassenger != null)
                {
                    currentPassenger = null;
                }
            }
        }

        private void OnAcceptButtonClicked()
        {
            uberJobActive = true;
            ShowNotification($"Ride accepted! Drive to pick up {currentPassenger.passengerName}\nat {currentPassenger.pickupAddress.DisplayName}.");
            CityManager.Instance.FindBuildingAndSetGuider(currentPassenger.pickupAddress.gameAddress, true);
        }

        private void OnDeclineButtonClicked()
        {
            uberJobActive = false;
            currentPassenger = null;
            showUberUI = true;
            lastJobCompletionTime = Time.time - UberJobCooldown + 60f;
            ShowNotification("Uber ride declined.");
            uiManager.UpdateUI(driverStats, currentPassenger, isPeakHours, uberJobActive);
        }

        private void OnCancelButtonClicked()
        {
            driverStats.cancelledRides++;
            uberJobActive = false;
            currentPassenger = null;
            showUberUI = true;
            ShowNotification("Uber ride canceled by driver.");
            SaveDriverStats();
            uiManager.UpdateUI(driverStats, currentPassenger, isPeakHours, uberJobActive);
            lastPassengerOfferTime = Time.time - PassengerOfferInterval + 15f;
        }

        public override void OnDeinitializeMelon()
        {
            if (uiManager != null)
            {
                uiManager.DestroyUI();
            }
        }

        #endregion

        #region Passenger and Job Logic
        private void OfferNewPassenger()
        {
            if (addresses.Count < 2) return;

            var pickup = addresses[UnityEngine.Random.Range(0, addresses.Count)];
            Address dropoff;
            do
            {
                dropoff = addresses[UnityEngine.Random.Range(0, addresses.Count)];
            } while (dropoff.gameAddress == pickup.gameAddress);

            if (!buildingPositions.ContainsKey(pickup.gameAddress) || !buildingPositions.ContainsKey(dropoff.gameAddress))
            {
                MelonLogger.Error("Could not find building positions for pickup or dropoff");
                return;
            }

            Vector3 pickupPos = buildingPositions[pickup.gameAddress];
            Vector3 dropoffPos = buildingPositions[dropoff.gameAddress];
            float distance = Vector3.Distance(pickupPos, dropoffPos);
            float fareMultiplier = isPeakHours ? PeakMultiplier : 1.0f;
            float fare = CalculateFare(distance) * fareMultiplier;

            PassengerType passengerType = (PassengerType)UnityEngine.Random.Range(0, 5);
            if (pickup.businessType == BusinessTypeName.JewelryStore || pickup.businessType == BusinessTypeName.Bank || pickup.businessType == BusinessTypeName.LawFirm)
                passengerType = UnityEngine.Random.Range(0, 100) < 50 ? PassengerType.Business : passengerType;
            else if (pickup.businessType == BusinessTypeName.Nightclub || pickup.businessType == BusinessTypeName.Casino)
                passengerType = UnityEngine.Random.Range(0, 100) < 50 ? PassengerType.Party : passengerType;
            else if (pickup.businessType == BusinessTypeName.GiftShop || pickup.businessType == BusinessTypeName.Florist)
                passengerType = UnityEngine.Random.Range(0, 100) < 50 ? PassengerType.Tourist : passengerType;
            else if (pickup.businessType == BusinessTypeName.Bookstore || pickup.businessType == BusinessTypeName.CoffeeShop)
                passengerType = UnityEngine.Random.Range(0, 100) < 50 ? PassengerType.Regular : passengerType;

            Gender passengerGender = UnityEngine.Random.Range(0, 2) == 0 ? Gender.Male : Gender.Female;
            string firstName = passengerGender == Gender.Male ?
                PassengerDialogues.MaleFirstNames[UnityEngine.Random.Range(0, PassengerDialogues.MaleFirstNames.Length)] :
                PassengerDialogues.FemaleFirstNames[UnityEngine.Random.Range(0, PassengerDialogues.FemaleFirstNames.Length)];
            string lastName = PassengerDialogues.LastNames[UnityEngine.Random.Range(0, PassengerDialogues.LastNames.Length)];
            string passengerName = $"{firstName} {lastName}";

            currentPassenger = new UberPassenger
            {
                pickupAddress = pickup,
                dropoffAddress = dropoff,
                pickupLocation = pickupPos,
                dropoffLocation = dropoffPos,
                fare = fare,
                isPickedUp = false,
                pickupTime = -1f,
                distanceToDestination = distance,
                passengerType = passengerType,
                passengerName = passengerName,
                conversationLines = PassengerDialogues.Comments[passengerType]["regular"]
                .SelectMany(kvp => kvp.Value) // Flatten the List<string> values
                .ToArray(), // Fix: Added the missing comma
                collisionCount = 0,
                suddenStopCount = 0,
                totalSpeedingTime = 0f,
                gender = passengerGender
            };

            string surgeMessage = isPeakHours ? " (Surge Pricing Active!)" : "";
            string message = $"New Uber Passenger Request!{surgeMessage}\n" +
                             $"Passenger: {passengerName}\n" +
                             $"Pickup: {pickup.DisplayName}, {pickup.neighborhood}\n" +
                             $"Dropoff: {dropoff.DisplayName}, {dropoff.neighborhood}\n" +
                             $"Estimated Fare: ${fare:F2}";
            ShowNotification(message);
            showUberUI = true;
        }

        private void ShowPassengerConversation()
        {
            if (currentPassenger == null || !currentPassenger.isPickedUp) return;

            if (currentPassenger.passengerType == PassengerType.Silent && UnityEngine.Random.Range(0, 100) < 90 && Time.time - lastSuddenStopTime >= 10f)
                return;

            string commentType = (conversationCount == 0 || !hadRegularConversation) ? "regular" : GetDrivingState();
            if (commentType == "regular") hadRegularConversation = true;

            float effectiveEnginePower = enginePower ?? 45f;
            VehicleController playerVehicle = GetPlayerVehicle();
            string vehicleName = playerVehicle?.GetName()?.ToString();
            bool isHighEndVehicle = currentVehicleCategory == VehicleCategory.Luxury || currentVehicleCategory == VehicleCategory.Performance;

            // Passenger gender-specific greeting
            string greeting = "";
            if (commentType == "regular" && (currentPassenger.passengerType == PassengerType.Business || currentPassenger.passengerType == PassengerType.Regular))
            {
                greeting = currentPassenger.gender == Gender.Male ? "Sir, " : "Ma’am, ";
            }

            // Check for flirty dialogue (opposite gender or Party passenger)
            bool isOppositeGender = playerGender != currentPassenger.gender;
            bool canFlirt = (currentPassenger.passengerType == PassengerType.Party) || (isOppositeGender && currentPassenger.passengerType != PassengerType.Silent);
            if (commentType == "regular" && canFlirt && UnityEngine.Random.Range(0, 100) < 15) // 15% chance for flirty dialogue
            {
                commentType = "flirty";
            }

            if (commentType == "regular" && UnityEngine.Random.Range(0, 100) < 25)
            {
                string vehicleComment = isHighEndVehicle && !string.IsNullOrEmpty(vehicleName) ?
                    currentPassenger.passengerType switch
                    {
                        PassengerType.Business => playerGender == Gender.Male ?
                            $"{greeting}This {vehicleName} is perfect for my meetings, man!" :
                            $"{greeting}This {vehicleName} is ideal for work, dear!",
                        PassengerType.Tourist => playerGender == Gender.Male ?
                            $"Wow, a {vehicleName}? This ride’s a trip highlight, dude!" :
                            $"A {vehicleName}? Loving this ride, hon!",
                        PassengerType.Party => playerGender == Gender.Male ?
                            $"Yo, a {vehicleName}? This car’s fire, bro!" :
                            $"Hey, a {vehicleName}? Total vibes, love!",
                        PassengerType.Silent => $"*nods approvingly at the {vehicleName}*",
                        _ => playerGender == Gender.Male ?
                            $"{greeting}Nice {vehicleName}, feels special, man!" :
                            $"{greeting}Great {vehicleName}, love the vibe, dear!"
                    } :
                    currentVehicleCategory switch
                    {
                        VehicleCategory.Luxury => currentPassenger.passengerType == PassengerType.Business ?
                            playerGender == Gender.Male ? $"{greeting}This car’s upscale, bro!" :
                            $"{greeting}So luxurious, hon!" : "Pure luxury ride!",
                        VehicleCategory.Economy => currentPassenger.passengerType == PassengerType.Tourist ?
                            effectiveEnginePower < 40f ? "Vibe’s cool but it’s slow!" :
                            "This car’s got charm!" :
                            effectiveEnginePower < 40f ? $"{greeting}We’re crawling, man!" :
                            $"{greeting}Hope it holds up, dear!",
                        VehicleCategory.Performance => currentPassenger.passengerType == PassengerType.Party ?
                            effectiveEnginePower > 80f ? "This car’s a beast, awesome!" :
                            "Cool ride, crank it up!" : "Feels fast just sitting here!",
                        _ => autoParkSupported ? $"{greeting}Solid car, neat tech!" :
                            $"{greeting}Good car, gets it done."
                    };

                ShowNotification($"{currentPassenger.passengerName}: \"{vehicleComment}\"", NotificationType.Info, 15f);
                conversationShown = true;
                conversationCount++;
                if (currentVehicleCategory == VehicleCategory.Luxury || currentVehicleCategory == VehicleCategory.Performance || autoParkSupported)
                    currentPassenger.positiveInteractions += (int)1f;
                else if (currentVehicleCategory == VehicleCategory.Economy || effectiveEnginePower < 40f)
                    currentPassenger.negativeInteractions += (int)0.5f;
                MelonCoroutines.Start(ResetConversationFlag());
                return;
            }

            if (commentType == "parking")
            {
                string parkingComment = isHighEndVehicle && !string.IsNullOrEmpty(vehicleName) ?
                    currentPassenger.passengerType switch
                    {
                        PassengerType.Business => playerGender == Gender.Male ?
                            $"{greeting}Auto-parking in a {vehicleName}? Efficient, man!" :
                            $"{greeting}That {vehicleName} parks itself? Nice, dear!",
                        PassengerType.Tourist => playerGender == Gender.Male ?
                            $"A {vehicleName} that parks? Sweet, dude!" :
                            $"Self-parking {vehicleName}? Cool, hon!",
                        PassengerType.Party => playerGender == Gender.Male ?
                            $"Smooth parking in a {vehicleName}, bro!" :
                            $"Nice park with that {vehicleName}, love!",
                        PassengerType.Silent => $"*small nod at the {vehicleName}’s tech*",
                        _ => playerGender == Gender.Male ?
                            $"{greeting}Great parking with this {vehicleName}, man!" :
                            $"{greeting}Smooth parking, dear!"
                    } :
                    currentPassenger.passengerType switch
                    {
                        PassengerType.Business => playerGender == Gender.Male ?
                            $"{greeting}Efficient auto-parking, bro!" :
                            $"{greeting}Love that parking tech, hon!",
                        PassengerType.Tourist => "The car parks itself? Amazing!",
                        PassengerType.Party => playerGender == Gender.Male ?
                            "Smooth parking, didn’t jolt us, man!" :
                            "Sweet parking, kept our vibe, love!",
                        PassengerType.Silent => "*small nod at the tech*",
                        _ => playerGender == Gender.Male ?
                            $"{greeting}Great parking, man!" :
                            $"{greeting}Nice parking, dear!"
                    };
                ShowNotification($"{currentPassenger.passengerName}: \"{parkingComment}\"", NotificationType.Info, 15f);
                conversationShown = true;
                conversationCount++;
                currentPassenger.positiveInteractions += (int)1f;
                MelonCoroutines.Start(ResetConversationFlag());
                return;
            }

            int randomChance = UnityEngine.Random.Range(0, 100);
            if (commentType == "sudden_stop" && randomChance >= 90) commentType = "regular";
            else if (commentType == "speeding" && randomChance >= 70) commentType = "regular";
            else if (commentType == "smooth" && randomChance >= 40) commentType = "regular";

            var comments = PassengerDialogues.Comments[currentPassenger.passengerType][commentType][playerGender];
            string conversation = comments[UnityEngine.Random.Range(0, comments.Count)];

            // Apply passenger gender greeting to regular comments for Business/Regular
            if (commentType == "regular" && (currentPassenger.passengerType == PassengerType.Business || currentPassenger.passengerType == PassengerType.Regular))
            {
                conversation = $"{greeting}{conversation}";
            }

            ShowNotification($"{currentPassenger.passengerName}: \"{conversation}\"", NotificationType.Info, 15f);
            conversationShown = true;
            conversationCount++;

            if (commentType == "smooth" || (commentType == "regular" && UnityEngine.Random.Range(0, 100) < 70))
            {
                currentPassenger.positiveInteractions++;
            }
            else if (commentType == "sudden_stop" || commentType == "speeding" || commentType == "collision")
            {
                currentPassenger.negativeInteractions++;
            }

            if (currentPassenger.passengerType == PassengerType.Tourist &&
                commentType == "regular" &&
                conversationCount >= 2 &&
                !currentPassenger.hadSpecialConversation &&
                UnityEngine.Random.Range(0, 100) < 40)
            {
                MelonCoroutines.Start(ShowSpecialTouristConversation());
            }

            MelonCoroutines.Start(ResetConversationFlag());
        }

        private IEnumerator ShowSpecialTouristConversation()
        {
            yield return new WaitForSeconds(8f);
            if (currentPassenger != null && currentPassenger.isPickedUp && currentPassenger.passengerType == PassengerType.Tourist)
            {
                string neighborhood = currentNeighborhood;
                string touristQuestion = $"Hey, what's the best place to visit in {neighborhood}? I'm new here!";
                ShowNotification($"{currentPassenger.passengerName}: \"{touristQuestion}\"", NotificationType.Info, 15f);
                currentPassenger.hadSpecialConversation = true;
                currentPassenger.positiveInteractions++;
            }
        }
        private void ShowCollisionReaction()
        {
            if (currentPassenger == null || !currentPassenger.isPickedUp || currentPassenger.collisionCount == 0) return;

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

        private void CheckDestinationReached(VehicleController playerVehicle)
        {
            if (playerVehicle == null || currentPassenger == null) return;

            if (!currentPassenger.isPickedUp)
            {
                float distanceToPickup = Vector3.Distance(playerVehicle.transform.position, currentPassenger.pickupLocation);
                bool isAtPickupAddress = IsPlayerAtAddress(currentPassenger.pickupAddress.gameAddress);
                if (distanceToPickup < CheckDistance || isAtPickupAddress)
                {
                    currentPassenger.isPickedUp = true;
                    currentPassenger.pickupTime = Time.time;
                    ShowNotification("Passenger picked up! Drive to the destination.");
                    CityManager.Instance.FindBuildingAndSetGuider(currentPassenger.dropoffAddress.gameAddress, true);
                }
            }
            else
            {
                float distanceToDropoff = Vector3.Distance(playerVehicle.transform.position, currentPassenger.dropoffLocation);
                bool isAtDropoffAddress = IsPlayerAtAddress(currentPassenger.dropoffAddress.gameAddress);
                if (distanceToDropoff < CheckDistance || isAtDropoffAddress)
                {
                    CompleteUberJob();
                }
            }
        }

        private void CompleteUberJob()
        {
            driverStats.totalRides++;
            driverStats.totalEarnings += currentPassenger.fare;
            if (currentPassenger.distanceToDestination > driverStats.longestRide)
                driverStats.longestRide = currentPassenger.distanceToDestination;

            string neighborhood = currentPassenger.dropoffAddress.neighborhood;
            if (!driverStats.neighborhoodVisits.ContainsKey(neighborhood))
                driverStats.neighborhoodVisits[neighborhood] = 0;
            driverStats.neighborhoodVisits[neighborhood]++;
            var mostVisited = driverStats.neighborhoodVisits.OrderByDescending(x => x.Value).FirstOrDefault();
            driverStats.mostVisitedNeighborhood = mostVisited.Key;

            // Calculate and add tip
            float tipAmount = CalculateTipAmount(currentPassenger);
            bool receivedTip = tipAmount > 0f;

            // Add fare to player's account
            AddMoneyToPlayer(currentPassenger.fare);

            // Add tip separately if received
            if (receivedTip)
            {
                AddMoneyToPlayer(tipAmount);
            }

            // Build completion message
            string completionMessage = $"Uber job completed! You earned ${currentPassenger.fare:F2} fare";
            if (receivedTip)
            {
                completionMessage += $"+ ${tipAmount:F2} tip!";
            }
            completionMessage += $"\n{currentPassenger.passengerName} has arrived at {currentPassenger.dropoffAddress.DisplayName}.";
            ShowNotification(completionMessage);
            hadRegularConversation = false;
            conversationCount = 0;

            ratingPending = true;
            uberJobActive = false;
            var completedPassenger = currentPassenger;
            currentPassenger = null;
            lastJobCompletionTime = Time.time;

            MelonCoroutines.Start(RequestRatingAfterDelay(3f, completedPassenger));
            showUberUI = true;
            SaveDriverStats();
        }

        private float CalculateTipAmount(UberPassenger passenger)
        {
            if (passenger == null || passenger.tipCalculated) return 0f;

            float baseTipPercentage = 0f;
            float baseTipRate = baselineTipRate;
            List<string> tipFactors = new List<string>();

            if (passengerTypeTipRates.ContainsKey(passenger.passengerType))
                baseTipRate = passengerTypeTipRates[passenger.passengerType];

            string dropoffNeighborhood = passenger.dropoffAddress.neighborhood;
            if (neighborhoodTipRates.ContainsKey(dropoffNeighborhood))
                baseTipRate = (baseTipRate + neighborhoodTipRates[dropoffNeighborhood]) / 2f;

            float tipChance = baseTipRate;

            if (passenger.collisionCount == 0 && passenger.suddenStopCount <= 1 && passenger.totalSpeedingTime < 15f)
            {
                tipChance *= tipMultiplierForGoodDriving;
                tipFactors.Add("smooth driving");
            }
            else if (passenger.collisionCount >= 2 || passenger.suddenStopCount >= 3 || passenger.totalSpeedingTime > 60f)
            {
                tipChance *= tipMultiplierForBadDriving;
            }

            float conversationFactor = 1.0f;
            if (passenger.positiveInteractions > 0)
            {
                conversationFactor += 0.1f * passenger.positiveInteractions;
                if (passenger.positiveInteractions >= 2)
                    tipFactors.Add("good conversation");
            }
            if (passenger.negativeInteractions > 0)
                conversationFactor -= 0.15f * passenger.negativeInteractions;

            tipChance *= conversationFactor;

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
                tipChance *= 1.3f;

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

            tipChance *= vehicleTipModifier;
            if (vehicleTipModifier > 1f) tipFactors.Add("nice car");
            else if (vehicleTipModifier < 1f) tipFactors.Add("budget ride");

            bool givesTip = UnityEngine.Random.Range(0f, 1f) < Mathf.Clamp(tipChance, 0.05f, 0.95f);

            if (!givesTip)
            {
                passenger.tipCalculated = true;
                return 0f;
            }

            baseTipPercentage = UnityEngine.Random.Range(0.1f, 0.3f);
            baseTipPercentage *= conversationFactor;

            baseTipPercentage *= currentVehicleCategory switch
            {
                VehicleCategory.Luxury => 1.3f,
                VehicleCategory.Performance => 1.1f,
                VehicleCategory.Economy => 0.9f,
                _ => 1f
            };

            if (autoParkSupported)
                baseTipPercentage += 0.02f;

            if (passenger.collisionCount == 0 && passenger.suddenStopCount == 0)
                baseTipPercentage += 0.05f;

            if (UnityEngine.Random.Range(0, 100) < 5)
            {
                baseTipPercentage = UnityEngine.Random.Range(0.4f, 0.5f);
                tipFactors.Clear();
                tipFactors.Add("extremely generous");
            }

            baseTipPercentage = Mathf.Clamp(baseTipPercentage, 0.05f, 0.5f);

            float tipAmount = passenger.fare * baseTipPercentage;
            tipAmount = Mathf.Round(tipAmount * 2) / 2;
            tipAmount = Mathf.Max(tipAmount, 1f);

            passenger.tipAmount = tipAmount;
            passenger.tipCalculated = true;

            driverStats.totalTipsEarned += tipAmount;
            driverStats.ridesWithTips++;
            driverStats.averageTipPercentage = ((driverStats.averageTipPercentage * (driverStats.ridesWithTips - 1)) + baseTipPercentage) / driverStats.ridesWithTips;

            if (tipAmount > driverStats.highestTip)
                driverStats.highestTip = tipAmount;

            if (!driverStats.tipsByPassengerType.ContainsKey(passenger.passengerType))
                driverStats.tipsByPassengerType[passenger.passengerType] = 0f;

            driverStats.tipsByPassengerType[passenger.passengerType] += tipAmount;

            return tipAmount;
        }
        private void PassengerCancelRide(string reason)
        {
            if (currentPassenger != null)
            {
                driverStats.cancelledRides++;
                ShowNotification(reason, NotificationType.Error);
                uberJobActive = false;
                currentPassenger = null;
                MelonCoroutines.Start(HideUIAfterDelay(5f));
                SaveDriverStats();
            }
        }

        #endregion

        #region Utility Methods

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

                string notificationId = "UberMod_" + UnityEngine.Random.Range(1000, 9999).ToString();
                if (message.Contains("completed")) notificationType = NotificationType.Success;
                else if (message.Contains("canceled")) notificationType = NotificationType.Error;
                if (message.Contains("Request") || message.Contains("accepted")) displayTime = 15f;
                notificationUI.ShowNotification(message, notificationType, displayTime);
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Failed to use game notification system: {ex.Message}");
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
                bool success = GameManager.ChangeMoneySafe(
                    amount,
                    transactionType,
                    dataHolder,
                    nullableDay,
                    currentPassenger?.dropoffAddress.gameAddress,
                    true
                );
            }
            catch (System.Exception ex)
            {
            }
        }

        private float CalculateFare(float distance)
        {
            float baseFare = 5.0f;
            float minutePerMeter = 0.04f;
            float pricePerMeter = 0.2f; // Was 0.15f
            float pricePerMinute = 0.07f; // Was 0.05f
            float estimatedTime = distance * minutePerMeter;
            float randomFactor = UnityEngine.Random.Range(0.95f, 1.05f); // Was 0.9f–1.1f

            // Passenger type modifier
            float passengerModifier = currentPassenger != null ? currentPassenger.passengerType switch
            {
                PassengerType.Business => 1.2f,
                PassengerType.Tourist => 1.1f,
                PassengerType.Party => 0.95f,
                PassengerType.Silent => 0.9f,
                _ => 1f
            } : 1f;

            // Vehicle category modifier
            float vehicleModifier = currentVehicleCategory switch
            {
                VehicleCategory.Luxury => 1.3f,
                VehicleCategory.Performance => 1.2f,
                VehicleCategory.Economy => 0.85f,
                _ => 1f
            };

            // Time modifier
            int currentHour = Il2Cpp.TimeHelper.CurrentHour;
            float timeModifier = (currentHour >= 22 || currentHour <= 4) ? 1.15f : 1f;

            float fare = (baseFare + (pricePerMeter * distance) + (pricePerMinute * estimatedTime)) *
                         randomFactor * passengerModifier * vehicleModifier * timeModifier;

            // Apply peak hours
            if (isPeakHours)
                fare *= PeakMultiplier;

            return Mathf.Max(fare, 2f); // Minimum fare
        }

        private bool IsPlayerAtAddress(Il2Cpp.Address targetAddress)
        {
            return lastClosestBuildingAddress != null && lastClosestBuildingAddress.Equals(targetAddress);
        }

        private void UpdateNeighborhood()
        {
            if (playerHUD != null && playerHUD._lastClosestBuilding != null)
            {
                lastClosestBuildingAddress = playerHUD._lastClosestBuilding.Address;
                currentNeighborhood = playerHUD._lastClosestBuilding.Neighbourhood.ToString();
            }
            else
            {
                currentNeighborhood = "Unknown";
            }
        }
        private string GetDrivingState()
        {
            // Prioritize recent collisions
            if (isCollision && currentPassenger.collisionCount > 0 && Time.time - lastCollisionTime < 10f && Time.time >= currentPassenger.pickupTime)
                return "collision";

            // Check for recent sudden stops (within 8s)
            if (Time.time - lastSuddenStopTime < 8f)
                return "sudden_stop";

            // Check for speeding (only if significant and recent)
            if (isSpeeding && currentPassenger.totalSpeedingTime > 15f && currentSpeed <= (maxVehicleSpeed ?? 100f) * 0.9f)
                return "speeding";

            // Smooth driving (increase threshold for more consistent triggering)
            if (smoothDrivingTime > 60f)
            {
                smoothDrivingTime = 0f;
                return "smooth";
            }

            // Parking (only if stopped and autoParkSupported)
            if (currentSpeed < 5f && currentPassenger.isPickedUp && Time.time - lastSuddenStopTime > 10f)
            {
                var brakes = GetPlayerVehicle()?.gameObject?.GetComponent<Il2CppNWH.VehiclePhysics2.Brakes>();
                if (brakes != null && brakes._isBraking && autoParkSupported && UnityEngine.Random.Range(0, 100) < 30)
                {
                    return "parking";
                }
            }

            return "regular";
        }
        private void UpdateDrivingState(VehicleController playerVehicle)
        {
            if (playerVehicle == null)
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
                return;
            }

            float currentTime = Time.time;
            currentSpeed = playerVehicle.CurrentSpeed;

            // Cache components
            var damageHandler = playerVehicle.gameObject?.GetComponent<Il2CppNWH.VehiclePhysics2.Damage.DamageHandler>();
            var speedLimiter = playerVehicle.gameObject?.GetComponent<Il2CppNWH.VehiclePhysics2.Modules.SpeedLimiter.SpeedLimiterModule>();

            // Vehicle type check
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
                            MelonLogger.Msg($"Vehicle detected: {lastVehicleTypeName}, maxSpeed={maxVehicleSpeed:F1}, category={currentVehicleCategory}");
                        }
                    }
                    else
                    {
                        maxVehicleSpeed = null;
                        vehiclePrice = null;
                        enginePower = null;
                        autoParkSupported = false;
                        currentVehicleCategory = VehicleCategory.Standard;
                        isCheapCar = false;
                        lastDetectedVehicle = playerVehicle;
                        lastVehicleTypeName = null;
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Error detecting vehicle type: {ex.Message}");
                    maxVehicleSpeed = null;
                    vehiclePrice = null;
                    enginePower = null;
                    autoParkSupported = false;
                    currentVehicleCategory = VehicleCategory.Standard;
                    isCheapCar = false;
                    lastDetectedVehicle = playerVehicle;
                    lastVehicleTypeName = null;
                }
            }

            float effectiveMaxSpeed = maxVehicleSpeed ?? 100f;
            float effectiveEnginePower = enginePower ?? 45f;
            float effectivePrice = vehiclePrice ?? 0f;

            float adjustedAccelerationThreshold = AccelerationThreshold * (effectiveEnginePower < 40f ? 0.8f : effectiveEnginePower > 80f ? 1.2f : 1f);
            dynamicSpeedLimit = CalculateDynamicSpeedLimit(effectiveMaxSpeed, currentVehicleCategory);

            // Adjust speeding tolerance based on vehicle type
            float adjustedSpeedingTolerance = speedingTolerance; // Default: 5%
            if (lastVehicleTypeName != null && lastVehicleTypeName.Contains("Honza Mimic"))
            {
                adjustedSpeedingTolerance = 10f; // Double tolerance for Honza Mimic (10%)
            }
            else if (currentVehicleCategory == VehicleCategory.Performance)
            {
                adjustedSpeedingTolerance = 8f; // Higher tolerance for Performance vehicles (8%)
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

            if (damageHandler != null)
            {
                float vehicleLastCollisionTime = damageHandler.lastCollisionTime;
                if (vehicleLastCollisionTime > lastCollisionTime &&
                    currentPassenger != null &&
                    currentPassenger.isPickedUp &&
                    vehicleLastCollisionTime >= currentPassenger.pickupTime)
                {
                    lastCollisionTime = vehicleLastCollisionTime;
                    isCollision = true;
                    currentPassenger.collisionCount++;
                    smoothDrivingTime = Mathf.Max(0, smoothDrivingTime - 60f);
                    float collisionPenalty = currentVehicleCategory == VehicleCategory.Luxury ? 1.5f : currentVehicleCategory == VehicleCategory.Performance ? 2f : 1f;
                    currentPassenger.negativeInteractions += (int)collisionPenalty;
                    if (currentPassenger.passengerType != PassengerType.Silent || UnityEngine.Random.Range(0, 100) < 80)
                    {
                        ShowCollisionReaction();
                    }
                }
                else if (isCollision && (currentTime - lastCollisionTime) > CollisionRecoveryTime)
                {
                    isCollision = false;
                }
            }

            bool isSuddenStop = false;

            if (previousTime > 0)
            {
                float deltaTime = currentTime - previousTime;
                float acceleration = (currentSpeed - previousSpeed) / deltaTime;

                // Check if a Sudden Stop condition is met
                if (previousSpeed > 10f && acceleration < -adjustedAccelerationThreshold)
                {
                    isSuddenStop = true;

                    // Only trigger notification and penalties if cooldown has expired
                    if (currentTime - lastSuddenStopTime >= suddenStopCooldown)
                    {
                        lastSuddenStopTime = currentTime;
                        suddenStopCooldown = SuddenStopCooldownDuration; // Reset cooldown
                        currentPassenger.suddenStopCount++;
                        float stopPenalty = currentVehicleCategory == VehicleCategory.Luxury ? 1.5f : 1f;
                        smoothDrivingTime = Mathf.Max(0, smoothDrivingTime - 20f * stopPenalty);
                        ShowDrivingNotification("sudden_stop", NotificationType.Warning, 3f);
                    }
                }

                // Update cooldown timer
                if (suddenStopCooldown > 0)
                {
                    suddenStopCooldown = Mathf.Max(0, suddenStopCooldown - deltaTime);
                }

                float effectiveSpeedLimit = dynamicSpeedLimit;
                float speedThreshold = effectiveSpeedLimit * (1f + adjustedSpeedingTolerance / 100f);
                isSpeeding = currentSpeed > speedThreshold;

                if (isSpeeding)
                {
                    float speedingFactor = currentVehicleCategory == VehicleCategory.Performance ? 0.5f : 1f;
                    currentPassenger.totalSpeedingTime += deltaTime * speedingFactor;

                    // Notify only if totalSpeedingTime exceeds 10f and cooldown has expired
                    if (currentPassenger.totalSpeedingTime > 10f && currentTime - lastSpeedingNotificationTime >= SpeedingNotificationCooldown)
                    {
                        lastSpeedingNotificationTime = currentTime;
                        ShowDrivingNotification("speeding", NotificationType.Warning, 3f);
                    }
                }
                else
                {
                    currentPassenger.totalSpeedingTime = Mathf.Max(0, currentPassenger.totalSpeedingTime - (deltaTime * 0.5f));
                }

                if (currentSpeed > 15f && Mathf.Abs(acceleration) < SmoothAccelerationThreshold && !isCollision && !isSuddenStop)
                    smoothDrivingTime += deltaTime;
                else if (currentSpeed < 5f)
                    smoothDrivingTime = Mathf.Max(0, smoothDrivingTime - (deltaTime * 0.2f));
            }

            previousSpeed = currentSpeed;
            previousTime = currentTime;
        }
        private void ShowDrivingNotification(string eventType, NotificationType notificationType, float displayTime)
        {
            try
            {
                if (notificationUI == null || currentPassenger == null)
                {
                    MelonLogger.Error("Cannot show driving notification: notificationUI or currentPassenger is null");
                    return;
                }

                if (!notificationMessages.ContainsKey(currentPassenger.passengerType) ||
                    !notificationMessages[currentPassenger.passengerType].ContainsKey(eventType))
                {
                    MelonLogger.Error($"No notification messages found for passengerType {currentPassenger.passengerType} and eventType {eventType}");
                    return;
                }

                var messages = notificationMessages[currentPassenger.passengerType][eventType];
                string message = messages[UnityEngine.Random.Range(0, messages.Count)];
                message = message.Replace("{playerName}", playerName);

                notificationUI.ShowNotification(message, notificationType, displayTime);
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Failed to show driving notification: {ex.Message}");
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
                // Use vehicleType.vehicleTypeName directly
                VehicleTypeName vehicleTypeName = vehicle.vehicleType.vehicleTypeName;

                // Access VehicleTypeHelper.VehicleTypes dictionary
                var vehicleTypes = Il2CppVehicles.VehicleTypes.VehicleTypeHelper.VehicleTypes;
                if (vehicleTypes == null)
                {
                    return null;
                }

                // Find matching VehicleType
                foreach (var pair in vehicleTypes)
                {
                    if (pair.Key == vehicleTypeName) // Compare using VehicleTypeName directly
                    {
                        return pair.Value;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        // Helper method to categorize vehicles
        private VehicleCategory CategorizeVehicle(VehicleType vehicleType)
        {
            if (vehicleType == null)
                return VehicleCategory.Standard;

            float price = vehicleType.price;
            float maxSpeed = vehicleType.maxSpeed;
            float enginePower = vehicleType.enginePower;
            bool autoParkSupported = vehicleType.autoParkSupported;

            // Categorization rules (maxSpeed capped at 100)
            if (price > 200000f || (enginePower > 80f && autoParkSupported) || vehicleType.name.ToLower().Contains("luxury"))
            {
                return VehicleCategory.Luxury; // High price, strong engine, auto-parking
            }
            else if (maxSpeed > 90f && enginePower > 60f && price > 200000f)
            {
                return VehicleCategory.Performance; // Fastest in game, decent power
            }
            else if (price < 5000f || maxSpeed < 70f || enginePower < 50f || !autoParkSupported)
            {
                return VehicleCategory.Economy; // Cheap, slow, weak, no auto-parking
            }
            else
            {
                return VehicleCategory.Standard; // Average stats
            }
        }
        private float CalculateDynamicSpeedLimit(float? maxSpeed, VehicleCategory category)
        {
            // Convert listed maxSpeed to actual in-game speed
            float actualMaxSpeed = (maxSpeed ?? 100f) * (22.0f / 100f); // Formula: (GetMaxSpeed / 100f) * 22.0f

            // Base speed limit on actual max speed, with multipliers to set a higher threshold
            float baseLimit = actualMaxSpeed * (category switch
            {
                VehicleCategory.Economy => 0.75f,    // Increased from 0.6f (~75% of actual max)
                VehicleCategory.Standard => 0.8f,    // Increased from 0.65f (~80% of actual max)
                VehicleCategory.Luxury => 0.9f,     // Increased from 0.7f (~85% of actual max)
                VehicleCategory.Performance => 0.9f  // Increased from 0.75f (~90% of actual max, e.g., 22.0 * 0.9 = 19.8 km/h)
            });

            float neighborhoodModifier = currentNeighborhood switch
            {
                "Midtown" or "LowerManhattan" or "HellsKitchen" => 0.85f,
                "MurrayHill" => 1.15f,
                _ => 1f
            };

            int currentHour = Il2Cpp.TimeHelper.CurrentHour;
            float timeModifier = (currentHour >= 22 || currentHour <= 4) ? 1.1f : 1f;

            float categoryModifier = category switch
            {
                VehicleCategory.Luxury => 0.95f,
                VehicleCategory.Performance => 1.1f,
                _ => 1f
            };

            float dynamicLimit = baseLimit * neighborhoodModifier * timeModifier * categoryModifier;
            float finalLimit = Mathf.Clamp(dynamicLimit, 5f, actualMaxSpeed * 0.95f); // Cap at 95% of actual max speed

            return finalLimit;
        }

        private void SaveDriverStats()
        {
            try
            {
                string saveDir = Path.Combine(MelonEnvironment.UserDataDirectory, "UberJobMod");
                string savePath = Path.Combine(saveDir, "DriverStats.json");
                if (!Directory.Exists(saveDir))
                    Directory.CreateDirectory(saveDir);
                string json = JsonConvert.SerializeObject(driverStats, Formatting.Indented);
                File.WriteAllText(savePath, json);
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error saving driver stats: {ex.Message}");
            }
        }

        public static string FormatDistance(float meters)
        {
            return meters >= 1000f ? $"{(meters / 1000f):0.0}km" : $"{meters:F0}m";
        }

        #endregion
    }
}