using System.Text.Json.Serialization;

namespace Prisma.Infrastructure.ExternalServices.BunnyApi.Core.PullZoneApi.GetPullZone;

public record GetPullZoneResponse
{
    [JsonPropertyName("Id")]
    public long Id { get; init; }

    [JsonPropertyName("Name")]
    public string? Name { get; init; }

    [JsonPropertyName("OriginUrl")]
    public string? OriginUrl { get; init; }

    [JsonPropertyName("Enabled")]
    public bool Enabled { get; init; }

    [JsonPropertyName("Suspended")]
    public bool Suspended { get; init; }

    [JsonPropertyName("Hostnames")]
    public List<HostnameModel>? Hostnames { get; init; }

    [JsonPropertyName("StorageZoneId")]
    public long StorageZoneId { get; init; }

    [JsonPropertyName("EdgeScriptId")]
    public long EdgeScriptId { get; init; }

    [JsonPropertyName("EdgeScriptExecutionPhase")]
    public ExecutionPhase EdgeScriptExecutionPhase { get; init; }

    [JsonPropertyName("MiddlewareScriptId")]
    public long? MiddlewareScriptId { get; init; }

    [JsonPropertyName("MagicContainersAppId")]
    public string? MagicContainersAppId { get; init; }

    [JsonPropertyName("MagicContainersEndpointId")]
    public string? MagicContainersEndpointId { get; init; }

    [JsonPropertyName("AllowedReferrers")]
    public List<string>? AllowedReferrers { get; init; }

    [JsonPropertyName("BlockedReferrers")]
    public List<string>? BlockedReferrers { get; init; }

    [JsonPropertyName("BlockedIps")]
    public List<string>? BlockedIps { get; init; }

    [JsonPropertyName("EnableGeoZoneUS")]
    public bool EnableGeoZoneUS { get; init; }

    [JsonPropertyName("EnableGeoZoneEU")]
    public bool EnableGeoZoneEU { get; init; }

    [JsonPropertyName("EnableGeoZoneASIA")]
    public bool EnableGeoZoneASIA { get; init; }

    [JsonPropertyName("EnableGeoZoneSA")]
    public bool EnableGeoZoneSA { get; init; }

    [JsonPropertyName("EnableGeoZoneAF")]
    public bool EnableGeoZoneAF { get; init; }

    [JsonPropertyName("IpFamilyPolicy")]
    public IpFamilyPolicy IpFamilyPolicy { get; init; }

    [JsonPropertyName("ZoneSecurityEnabled")]
    public bool ZoneSecurityEnabled { get; init; }

    [JsonPropertyName("ZoneSecurityKey")]
    public string? ZoneSecurityKey { get; init; }

    [JsonPropertyName("ZoneSecurityIncludeHashRemoteIP")]
    public bool ZoneSecurityIncludeHashRemoteIP { get; init; }

    [JsonPropertyName("IgnoreQueryStrings")]
    public bool IgnoreQueryStrings { get; init; }

    [JsonPropertyName("MonthlyBandwidthLimit")]
    public long MonthlyBandwidthLimit { get; init; }

    [JsonPropertyName("MonthlyBandwidthUsed")]
    public long MonthlyBandwidthUsed { get; init; }

    [JsonPropertyName("MonthlyCharges")]
    public decimal MonthlyCharges { get; init; }

    [JsonPropertyName("AddHostHeader")]
    public bool AddHostHeader { get; init; }

    [JsonPropertyName("OriginHostHeader")]
    public string? OriginHostHeader { get; init; }

    [JsonPropertyName("Type")]
    public PullZoneType Type { get; init; }

    [JsonPropertyName("AccessControlOriginHeaderExtensions")]
    public List<string>? AccessControlOriginHeaderExtensions { get; init; }

    [JsonPropertyName("EnableAccessControlOriginHeader")]
    public bool EnableAccessControlOriginHeader { get; init; }

    [JsonPropertyName("DisableCookies")]
    public bool DisableCookies { get; init; }

    [JsonPropertyName("BudgetRedirectedCountries")]
    public List<string>? BudgetRedirectedCountries { get; init; }

    [JsonPropertyName("BlockedCountries")]
    public List<string>? BlockedCountries { get; init; }

    [JsonPropertyName("EnableOriginShield")]
    public bool EnableOriginShield { get; init; }

    [JsonPropertyName("CacheControlMaxAgeOverride")]
    public long CacheControlMaxAgeOverride { get; init; }

    [JsonPropertyName("CacheControlPublicMaxAgeOverride")]
    public long CacheControlPublicMaxAgeOverride { get; init; }

    [JsonPropertyName("BurstSize")]
    public int BurstSize { get; init; }

    [JsonPropertyName("RequestLimit")]
    public int RequestLimit { get; init; }

    [JsonPropertyName("BlockRootPathAccess")]
    public bool BlockRootPathAccess { get; init; }

    [JsonPropertyName("BlockPostRequests")]
    public bool BlockPostRequests { get; init; }

    [JsonPropertyName("LimitRatePerSecond")]
    public double LimitRatePerSecond { get; init; }

    [JsonPropertyName("LimitRateAfter")]
    public double LimitRateAfter { get; init; }

    [JsonPropertyName("ConnectionLimitPerIPCount")]
    public int ConnectionLimitPerIPCount { get; init; }

    [JsonPropertyName("PriceOverride")]
    public decimal PriceOverride { get; init; }

    [JsonPropertyName("OptimizerPricing")]
    public decimal OptimizerPricing { get; init; }

    [JsonPropertyName("AddCanonicalHeader")]
    public bool AddCanonicalHeader { get; init; }

    [JsonPropertyName("EnableLogging")]
    public bool EnableLogging { get; init; }

    [JsonPropertyName("EnableCacheSlice")]
    public bool EnableCacheSlice { get; init; }

    [JsonPropertyName("EnableSmartCache")]
    public bool EnableSmartCache { get; init; }

    [JsonPropertyName("EdgeRules")]
    public List<EdgeRuleV2Model>? EdgeRules { get; init; }

    [JsonPropertyName("EnableWebPVary")]
    public bool EnableWebPVary { get; init; }

    [JsonPropertyName("EnableAvifVary")]
    public bool EnableAvifVary { get; init; }

    [JsonPropertyName("EnableCountryCodeVary")]
    public bool EnableCountryCodeVary { get; init; }

    [JsonPropertyName("EnableCountryStateCodeVary")]
    public bool EnableCountryStateCodeVary { get; init; }

    [JsonPropertyName("EnableMobileVary")]
    public bool EnableMobileVary { get; init; }

    [JsonPropertyName("EnableCookieVary")]
    public bool EnableCookieVary { get; init; }

    [JsonPropertyName("CookieVaryParameters")]
    public List<string>? CookieVaryParameters { get; init; }

    [JsonPropertyName("EnableHostnameVary")]
    public bool EnableHostnameVary { get; init; }

    [JsonPropertyName("CnameDomain")]
    public string? CnameDomain { get; init; }

    [JsonPropertyName("AWSSigningEnabled")]
    public bool AWSSigningEnabled { get; init; }

    [JsonPropertyName("AWSSigningKey")]
    public string? AWSSigningKey { get; init; }

    [JsonPropertyName("AWSSigningSecret")]
    public string? AWSSigningSecret { get; init; }

    [JsonPropertyName("AWSSigningRegionName")]
    public string? AWSSigningRegionName { get; init; }

    [JsonPropertyName("LoggingIPAnonymizationEnabled")]
    public bool LoggingIPAnonymizationEnabled { get; init; }

    [JsonPropertyName("EnableTLS1")]
    public bool EnableTLS1 { get; init; }

    [JsonPropertyName("EnableTLS1_1")]
    public bool EnableTLS1_1 { get; init; }

    [JsonPropertyName("TlsSecurityLevel")]
    public TlsSecurityLevelType TlsSecurityLevel { get; init; }

    [JsonPropertyName("VerifyOriginSSL")]
    public bool VerifyOriginSSL { get; init; }

    [JsonPropertyName("ErrorPageEnableCustomCode")]
    public bool ErrorPageEnableCustomCode { get; init; }

    [JsonPropertyName("ErrorPageCustomCode")]
    public string? ErrorPageCustomCode { get; init; }

    [JsonPropertyName("ErrorPageEnableStatuspageWidget")]
    public bool ErrorPageEnableStatuspageWidget { get; init; }

    [JsonPropertyName("ErrorPageStatuspageCode")]
    public string? ErrorPageStatuspageCode { get; init; }

    [JsonPropertyName("ErrorPageWhitelabel")]
    public bool ErrorPageWhitelabel { get; init; }

    [JsonPropertyName("OriginShieldZoneCode")]
    public string? OriginShieldZoneCode { get; init; }

    [JsonPropertyName("LogForwardingEnabled")]
    public bool LogForwardingEnabled { get; init; }

    [JsonPropertyName("LogForwardingHostname")]
    public string? LogForwardingHostname { get; init; }

    [JsonPropertyName("LogForwardingPort")]
    public int LogForwardingPort { get; init; }

    [JsonPropertyName("LogForwardingToken")]
    public string? LogForwardingToken { get; init; }

    [JsonPropertyName("LogForwardingProtocol")]
    public PullZoneLogForwarderProtocolType LogForwardingProtocol { get; init; }

    [JsonPropertyName("LoggingSaveToStorage")]
    public bool LoggingSaveToStorage { get; init; }

    [JsonPropertyName("LoggingStorageZoneId")]
    public long LoggingStorageZoneId { get; init; }

    [JsonPropertyName("FollowRedirects")]
    public bool FollowRedirects { get; init; }

    [JsonPropertyName("VideoLibraryId")]
    public long VideoLibraryId { get; init; }

    [JsonPropertyName("DnsRecordId")]
    public long DnsRecordId { get; init; }

    [JsonPropertyName("DnsZoneId")]
    public long DnsZoneId { get; init; }

    [JsonPropertyName("DnsRecordValue")]
    public string? DnsRecordValue { get; init; }

    [JsonPropertyName("OptimizerEnabled")]
    public bool OptimizerEnabled { get; init; }

    [JsonPropertyName("OptimizerTunnelEnabled")]
    public bool OptimizerTunnelEnabled { get; init; }

    [JsonPropertyName("OptimizerTunnelActiveProbingEnabled")]
    public bool OptimizerTunnelActiveProbingEnabled { get; init; }

    [JsonPropertyName("OptimizerDesktopMaxWidth")]
    public int OptimizerDesktopMaxWidth { get; init; }

    [JsonPropertyName("OptimizerMobileMaxWidth")]
    public int OptimizerMobileMaxWidth { get; init; }

    [JsonPropertyName("OptimizerImageQuality")]
    public int OptimizerImageQuality { get; init; }

    [JsonPropertyName("OptimizerMobileImageQuality")]
    public int OptimizerMobileImageQuality { get; init; }

    [JsonPropertyName("OptimizerEnableWebP")]
    public bool OptimizerEnableWebP { get; init; }

    [JsonPropertyName("OptimizerPrerenderHtml")]
    public bool OptimizerPrerenderHtml { get; init; }

    [JsonPropertyName("OptimizerHtmlToMarkdown")]
    public OptimizerHtmlToMarkdownType OptimizerHtmlToMarkdown { get; init; }

    [JsonPropertyName("OptimizerLlmsTxt")]
    public bool OptimizerLlmsTxt { get; init; }

    [JsonPropertyName("OptimizerEnableManipulationEngine")]
    public bool OptimizerEnableManipulationEngine { get; init; }

    [JsonPropertyName("OptimizerMinifyCSS")]
    public bool OptimizerMinifyCSS { get; init; }

    [JsonPropertyName("OptimizerMinifyJavaScript")]
    public bool OptimizerMinifyJavaScript { get; init; }

    [JsonPropertyName("OptimizerWatermarkEnabled")]
    public bool OptimizerWatermarkEnabled { get; init; }

    [JsonPropertyName("OptimizerWatermarkUrl")]
    public string? OptimizerWatermarkUrl { get; init; }

    [JsonPropertyName("OptimizerWatermarkPosition")]
    public OptimizerWatermarkPosition OptimizerWatermarkPosition { get; init; }

    [JsonPropertyName("OptimizerWatermarkOffset")]
    public double OptimizerWatermarkOffset { get; init; }

    [JsonPropertyName("OptimizerWatermarkMinImageSize")]
    public int OptimizerWatermarkMinImageSize { get; init; }

    [JsonPropertyName("OptimizerAutomaticOptimizationEnabled")]
    public bool OptimizerAutomaticOptimizationEnabled { get; init; }

    [JsonPropertyName("PermaCacheStorageZoneId")]
    public long PermaCacheStorageZoneId { get; init; }

    [JsonPropertyName("PermaCacheType")]
    public PermaCacheType PermaCacheType { get; init; }

    [JsonPropertyName("OriginRetries")]
    public int OriginRetries { get; init; }

    [JsonPropertyName("OriginConnectTimeout")]
    public int OriginConnectTimeout { get; init; }

    [JsonPropertyName("OriginResponseTimeout")]
    public int OriginResponseTimeout { get; init; }

    [JsonPropertyName("UseStaleWhileUpdating")]
    public bool UseStaleWhileUpdating { get; init; }

    [JsonPropertyName("UseStaleWhileOffline")]
    public bool UseStaleWhileOffline { get; init; }

    [JsonPropertyName("OriginRetry5XXResponses")]
    public bool OriginRetry5XXResponses { get; init; }

    [JsonPropertyName("OriginRetryConnectionTimeout")]
    public bool OriginRetryConnectionTimeout { get; init; }

    [JsonPropertyName("OriginRetryResponseTimeout")]
    public bool OriginRetryResponseTimeout { get; init; }

    [JsonPropertyName("OriginRetryDelay")]
    public int OriginRetryDelay { get; init; }

    [JsonPropertyName("QueryStringVaryParameters")]
    public List<string>? QueryStringVaryParameters { get; init; }

    [JsonPropertyName("OriginShieldEnableConcurrencyLimit")]
    public bool OriginShieldEnableConcurrencyLimit { get; init; }

    [JsonPropertyName("OriginShieldMaxConcurrentRequests")]
    public int OriginShieldMaxConcurrentRequests { get; init; }

    [JsonPropertyName("EnableSafeHop")]
    public bool EnableSafeHop { get; init; }

    [JsonPropertyName("CacheErrorResponses")]
    public bool CacheErrorResponses { get; init; }

    [JsonPropertyName("OriginShieldQueueMaxWaitTime")]
    public int OriginShieldQueueMaxWaitTime { get; init; }

    [JsonPropertyName("OriginShieldMaxQueuedRequests")]
    public int OriginShieldMaxQueuedRequests { get; init; }

    [JsonPropertyName("OptimizerClasses")]
    public List<OptimizerClassModel>? OptimizerClasses { get; init; }

    [JsonPropertyName("OptimizerForceClasses")]
    public bool OptimizerForceClasses { get; init; }

    [JsonPropertyName("OptimizerStaticHtmlEnabled")]
    public bool OptimizerStaticHtmlEnabled { get; init; }

    [JsonPropertyName("OptimizerStaticHtmlWordPressPath")]
    public string? OptimizerStaticHtmlWordPressPath { get; init; }

    [JsonPropertyName("OptimizerStaticHtmlWordPressBypassCookie")]
    public string? OptimizerStaticHtmlWordPressBypassCookie { get; init; }

    [JsonPropertyName("UseBackgroundUpdate")]
    public bool UseBackgroundUpdate { get; init; }

    [JsonPropertyName("EnableAutoSSL")]
    public bool EnableAutoSSL { get; init; }

    [JsonPropertyName("EnableQueryStringOrdering")]
    public bool EnableQueryStringOrdering { get; init; }

    [JsonPropertyName("LogAnonymizationType")]
    public LogAnonymizationType LogAnonymizationType { get; init; }

    [JsonPropertyName("LogFormat")]
    public PullZoneLogFormat LogFormat { get; init; }

    [JsonPropertyName("LogForwardingFormat")]
    public PullZoneLogFormat LogForwardingFormat { get; init; }

    [JsonPropertyName("ShieldDDosProtectionType")]
    public ShieldDDosProtectionType ShieldDDosProtectionType { get; init; }

    [JsonPropertyName("ShieldDDosProtectionEnabled")]
    public bool ShieldDDosProtectionEnabled { get; init; }

    [JsonPropertyName("OriginType")]
    public PullZoneOriginType OriginType { get; init; }

    [JsonPropertyName("EnableRequestCoalescing")]
    public bool EnableRequestCoalescing { get; init; }

    [JsonPropertyName("RequestCoalescingTimeout")]
    public int RequestCoalescingTimeout { get; init; }

    [JsonPropertyName("OriginLinkValue")]
    public string? OriginLinkValue { get; init; }

    [JsonPropertyName("DisableLetsEncrypt")]
    public bool DisableLetsEncrypt { get; init; }

    [JsonPropertyName("EnableBunnyImageAi")]
    public bool EnableBunnyImageAi { get; init; }

    [JsonPropertyName("BunnyAiImageBlueprints")]
    public List<BunnyAiImageBlueprintModel>? BunnyAiImageBlueprints { get; init; }

    [JsonPropertyName("PreloadingScreenEnabled")]
    public bool PreloadingScreenEnabled { get; init; }

    [JsonPropertyName("PreloadingScreenShowOnFirstVisit")]
    public bool PreloadingScreenShowOnFirstVisit { get; init; }

    [JsonPropertyName("PreloadingScreenCode")]
    public string? PreloadingScreenCode { get; init; }

    [JsonPropertyName("PreloadingScreenLogoUrl")]
    public string? PreloadingScreenLogoUrl { get; init; }

    [JsonPropertyName("PreloadingScreenCodeEnabled")]
    public bool PreloadingScreenCodeEnabled { get; init; }

    [JsonPropertyName("PreloadingScreenTheme")]
    public PreloadingScreenTheme PreloadingScreenTheme { get; init; }

    [JsonPropertyName("PreloadingScreenDelay")]
    public int PreloadingScreenDelay { get; init; }

    [JsonPropertyName("EUUSDiscount")]
    public int EUUSDiscount { get; init; }

    [JsonPropertyName("SouthAmericaDiscount")]
    public int SouthAmericaDiscount { get; init; }

    [JsonPropertyName("AfricaDiscount")]
    public int AfricaDiscount { get; init; }

    [JsonPropertyName("AsiaOceaniaDiscount")]
    public int AsiaOceaniaDiscount { get; init; }

    [JsonPropertyName("RoutingFilters")]
    public List<string>? RoutingFilters { get; init; }

    [JsonPropertyName("BlockNoneReferrer")]
    public bool BlockNoneReferrer { get; init; }

    [JsonPropertyName("StickySessionType")]
    public StickySessionType StickySessionType { get; init; }

    [JsonPropertyName("StickySessionCookieName")]
    public string? StickySessionCookieName { get; init; }

    [JsonPropertyName("StickySessionClientHeaders")]
    public string? StickySessionClientHeaders { get; init; }

    [JsonPropertyName("LoadBalancerId")]
    public long? LoadBalancerId { get; init; }

    [JsonPropertyName("UserId")]
    public string? UserId { get; init; }

    [JsonPropertyName("CacheVersion")]
    public long CacheVersion { get; init; }

    [JsonPropertyName("OptimizerEnableUpscaling")]
    public bool OptimizerEnableUpscaling { get; init; }

    [JsonPropertyName("EnableWebSockets")]
    public bool EnableWebSockets { get; init; }

    [JsonPropertyName("MaxWebSocketConnections")]
    public int MaxWebSocketConnections { get; init; }

    [JsonPropertyName("EnableExtendedLogging")]
    public bool EnableExtendedLogging { get; init; }

    [JsonPropertyName("CacheKeyHeaders")]
    public string? CacheKeyHeaders { get; init; }
}

// --- Nested Complex Types ---

public record HostnameModel
{
    [JsonPropertyName("Id")]
    public long Id { get; init; }

    [JsonPropertyName("Value")]
    public string? Value { get; init; }

    [JsonPropertyName("ForceSSL")]
    public bool ForceSSL { get; init; }

    [JsonPropertyName("IsSystemHostname")]
    public bool IsSystemHostname { get; init; }

    [JsonPropertyName("IsManagedHostname")]
    public bool IsManagedHostname { get; init; }

    [JsonPropertyName("HasCertificate")]
    public bool HasCertificate { get; init; }

    [JsonPropertyName("Certificate")]
    public string? Certificate { get; init; }

    [JsonPropertyName("CertificateKey")]
    public string? CertificateKey { get; init; }

    [JsonPropertyName("CertificateKeyType")]
    public PrivateKeyType CertificateKeyType { get; init; }

    [JsonPropertyName("CertificateProvisionType")]
    public CertificateProvisionType CertificateProvisionType { get; init; }
}

public record EdgeRuleV2Model
{
    [JsonPropertyName("Guid")]
    public string? Guid { get; init; }

    [JsonPropertyName("ActionType")]
    public EdgeRuleActionType ActionType { get; init; }

    [JsonPropertyName("ActionParameter1")]
    public string? ActionParameter1 { get; init; }

    [JsonPropertyName("ActionParameter2")]
    public string? ActionParameter2 { get; init; }

    [JsonPropertyName("ActionParameter3")]
    public string? ActionParameter3 { get; init; }

    [JsonPropertyName("Triggers")]
    public List<Trigger>? Triggers { get; init; }

    [JsonPropertyName("ExtraActions")]
    public List<EdgeRuleV2ActionModel>? ExtraActions { get; init; }

    [JsonPropertyName("TriggerMatchingType")]
    public TriggerMatchingTypes TriggerMatchingType { get; init; }

    [JsonPropertyName("Description")]
    public string? Description { get; init; }

    [JsonPropertyName("Enabled")]
    public bool Enabled { get; init; }

    [JsonPropertyName("OrderIndex")]
    public int OrderIndex { get; init; }

    [JsonPropertyName("ReadOnly")]
    public bool ReadOnly { get; init; }
}

public record Trigger
{
    [JsonPropertyName("Type")]
    public TriggerTypes Type { get; init; }

    [JsonPropertyName("PatternMatches")]
    public List<string>? PatternMatches { get; init; }

    [JsonPropertyName("PatternMatchingType")]
    public PatternMatchingTypes PatternMatchingType { get; init; }

    [JsonPropertyName("Parameter1")]
    public string? Parameter1 { get; init; }
}

public record EdgeRuleV2ActionModel
{
    [JsonPropertyName("ActionType")]
    public EdgeRuleActionType ActionType { get; init; }

    [JsonPropertyName("ActionParameter1")]
    public string? ActionParameter1 { get; init; }

    [JsonPropertyName("ActionParameter2")]
    public string? ActionParameter2 { get; init; }

    [JsonPropertyName("ActionParameter3")]
    public string? ActionParameter3 { get; init; }
}

public record OptimizerClassModel
{
    [JsonPropertyName("Name")]
    public string? Name { get; init; }

    [JsonPropertyName("Properties")]
    public Dictionary<string, string>? Properties { get; init; }
}

public record BunnyAiImageBlueprintModel
{
    [JsonPropertyName("Name")]
    public string? Name { get; init; }

    [JsonPropertyName("Properties")]
    public Dictionary<string, string>? Properties { get; init; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ExecutionPhase
{
    Cache = 0,
    LoadBalancer = 1,
    PreCache = 2,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IpFamilyPolicy
{
    IPv4Only = 0,
    DualStack = 1,
    DualStackPreferIPv6 = 2,
    IPv6Only = 3,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PullZoneType
{
    Premium = 0,
    Volume = 1,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TlsSecurityLevelType
{
    Legacy = 0,
    Compatible = 1,
    ModernOnly = 2,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PullZoneLogForwarderProtocolType
{
    UDP = 0,
    TCP = 1,
    TCPEncrypted = 2,
    DataDog = 3,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OptimizerHtmlToMarkdownType
{
    Off = 0,
    Simple = 1,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OptimizerWatermarkPosition
{
    BottomLeft = 0,
    BottomRight = 1,
    TopLeft = 2,
    TopRight = 3,
    Center = 4,
    CenterStretch = 5,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PermaCacheType
{
    Automatic = 0,
    Manual = 1,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LogAnonymizationType
{
    OneDigit = 0,
    Drop = 1,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PullZoneLogFormat
{
    Plain = 0,
    JSON = 1,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ShieldDDosProtectionType
{
    DetectOnly = 0,
    ActiveStandard = 1,
    ActiveAggressive = 2,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PullZoneOriginType
{
    OriginUrl = 0,
    DnsAccelerate = 1,
    StorageZone = 2,
    LoadBalancer = 3,
    EdgeScript = 4,
    MagicContainers = 5,
    PushZone = 6,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PreloadingScreenTheme
{
    Light = 0,
    Dark = 1,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StickySessionType
{
    Off = 0,
    On = 1,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PrivateKeyType
{
    Ecdsa = 0,
    Rsa = 1,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CertificateProvisionType
{
    Unknown = 0,
    Http01 = 1,
    Dns01 = 2,
    Custom = 3,
    Managed = 4,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EdgeRuleActionType
{
    ForceSSL = 0,
    Redirect = 1,
    OriginUrl = 2,
    OverrideCacheTime = 3,
    BlockRequest = 4,
    SetResponseHeader = 5,
    SetRequestHeader = 6,
    ForceDownload = 7,
    DisableTokenAuthentication = 8,
    EnableTokenAuthentication = 9,
    OverrideCacheTimePublic = 10,
    IgnoreQueryString = 11,
    DisableOptimizer = 12,
    ForceCompression = 13,
    SetStatusCode = 14,
    BypassPermaCache = 15,
    OverrideBrowserCacheTime = 16,
    OriginStorage = 17,
    SetNetworkRateLimit = 18,
    SetConnectionLimit = 19,
    SetRequestsPerSecondLimit = 20,
    RunEdgeScript = 21,
    OriginMagicContainers = 22,
    DisableWAF = 23,
    RetryOrigin = 24,
    OverrideBrowserCacheResponseHeader = 25,
    RemoveBrowserCacheResponseHeader = 26,
    DisableShieldChallenge = 27,
    DisableShield = 28,
    DisableShieldBotDetection = 29,
    BypassAwsS3Authentication = 30,
    DisableShieldAccessLists = 31,
    DisableShieldRateLimiting = 32,
    EnableRequestCoalescing = 33,
    DisableRequestCoalescing = 34,
    UseLoadBalancer = 35,
    ForceLoadBalancerStickySessionMode = 36,
    StripCookiesClientToOrigin = 37,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TriggerMatchingTypes
{
    MatchAny = 0,
    MatchAll = 1,
    MatchNone = 2,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TriggerTypes
{
    Url = 0,
    RequestHeader = 1,
    ResponseHeader = 2,
    UrlExtension = 3,
    CountryCode = 4,
    RemoteIP = 5,
    UrlQueryString = 6,
    RandomChance = 7,
    StatusCode = 8,
    RequestMethod = 9,
    CookieValue = 10,
    CountryStateCode = 11,
    OriginRetryAttemptCount = 12,
    OriginConnectionError = 13,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PatternMatchingTypes
{
    MatchAny = 0,
    MatchAll = 1,
    MatchNone = 2,
}
