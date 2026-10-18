using System.Text.Json.Serialization;

namespace Prisma.Infrastructure.ExternalServices.BunnyApi.Core.SharedDtos;

public record VideoLibraryModel
{
    [JsonPropertyName("Id")]
    public long Id { get; init; }

    [JsonPropertyName("Name")]
    public string? Name { get; init; }

    [JsonPropertyName("VideoCount")]
    public long VideoCount { get; init; }

    [JsonPropertyName("TrafficUsage")]
    public long TrafficUsage { get; init; }

    [JsonPropertyName("StorageUsage")]
    public long StorageUsage { get; init; }

    [JsonPropertyName("DateCreated")]
    public DateTime DateCreated { get; init; }

    [JsonPropertyName("DateModified")]
    public DateTime DateModified { get; init; }

    [JsonPropertyName("ReplicationRegions")]
    public List<string>? ReplicationRegions { get; init; }

    [JsonPropertyName("ApiKey")]
    public string? ApiKey { get; init; }

    [JsonPropertyName("ReadOnlyApiKey")]
    public string? ReadOnlyApiKey { get; init; }

    [JsonPropertyName("HasWatermark")]
    public bool HasWatermark { get; init; }

    [JsonPropertyName("WatermarkPositionLeft")]
    public int WatermarkPositionLeft { get; init; }

    [JsonPropertyName("WatermarkPositionTop")]
    public int WatermarkPositionTop { get; init; }

    [JsonPropertyName("WatermarkWidth")]
    public int WatermarkWidth { get; init; }

    [JsonPropertyName("PullZoneId")]
    public long PullZoneId { get; init; }

    [JsonPropertyName("StorageZoneId")]
    public long StorageZoneId { get; init; }

    [JsonPropertyName("WatermarkHeight")]
    public int WatermarkHeight { get; init; }

    [JsonPropertyName("EnabledResolutions")]
    public string? EnabledResolutions { get; init; }

    [JsonPropertyName("ViAiPublisherId")]
    public string? ViAiPublisherId { get; init; }

    [JsonPropertyName("VastTagUrl")]
    public string? VastTagUrl { get; init; }

    [JsonPropertyName("WebhookUrl")]
    public string? WebhookUrl { get; init; }

    [JsonPropertyName("CaptionsFontSize")]
    public int CaptionsFontSize { get; init; }

    [JsonPropertyName("CaptionsFontColor")]
    public string? CaptionsFontColor { get; init; }

    [JsonPropertyName("CaptionsBackground")]
    public string? CaptionsBackground { get; init; }

    [JsonPropertyName("UILanguage")]
    public string? UILanguage { get; init; }

    [JsonPropertyName("AllowEarlyPlay")]
    public bool? AllowEarlyPlay { get; init; }

    [JsonPropertyName("PlayerTokenAuthenticationEnabled")]
    public bool PlayerTokenAuthenticationEnabled { get; init; }

    [JsonPropertyName("AllowedReferrers")]
    public List<string>? AllowedReferrers { get; init; }

    [JsonPropertyName("BlockedReferrers")]
    public List<string>? BlockedReferrers { get; init; }

    [JsonPropertyName("BlockNoneReferrer")]
    public bool BlockNoneReferrer { get; init; }

    [JsonPropertyName("EnableMP4Fallback")]
    public bool EnableMP4Fallback { get; init; }

    [JsonPropertyName("KeepOriginalFiles")]
    public bool KeepOriginalFiles { get; init; }

    [JsonPropertyName("AllowDirectPlay")]
    public bool AllowDirectPlay { get; init; }

    [JsonPropertyName("EnableDRM")]
    public bool EnableDRM { get; init; }

    [JsonPropertyName("DrmVersion")]
    public DrmVersion DrmVersion { get; init; }

    [JsonPropertyName("AppleFairPlayDrm")]
    public AppleFairPlayDrm? AppleFairPlayDrm { get; init; }

    [JsonPropertyName("GoogleWidevineDrm")]
    public GoogleWidevineDrm? GoogleWidevineDrm { get; init; }

    [JsonPropertyName("Bitrate240p")]
    public int Bitrate240p { get; init; }

    [JsonPropertyName("Bitrate360p")]
    public int Bitrate360p { get; init; }

    [JsonPropertyName("Bitrate480p")]
    public int Bitrate480p { get; init; }

    [JsonPropertyName("Bitrate720p")]
    public int Bitrate720p { get; init; }

    [JsonPropertyName("Bitrate1080p")]
    public int Bitrate1080p { get; init; }

    [JsonPropertyName("Bitrate1440p")]
    public int Bitrate1440p { get; init; }

    [JsonPropertyName("Bitrate2160p")]
    public int Bitrate2160p { get; init; }

    [JsonPropertyName("ApiAccessKey")]
    [Obsolete(
        "The API access key for the library. Should use the ApiKey as the value is the same."
    )]
    public string? ApiAccessKey { get; init; }

    [JsonPropertyName("ShowHeatmap")]
    public bool ShowHeatmap { get; init; }

    [JsonPropertyName("EnableContentTagging")]
    public bool EnableContentTagging { get; init; }

    [JsonPropertyName("PullZoneType")]
    public PullZoneType PullZoneType { get; init; }

    [JsonPropertyName("CustomHTML")]
    public string? CustomHTML { get; init; }

    [JsonPropertyName("Controls")]
    public string? Controls { get; init; }

    [JsonPropertyName("PlaybackSpeeds")]
    public string? PlaybackSpeeds { get; init; }

    [JsonPropertyName("PlayerKeyColor")]
    public string? PlayerKeyColor { get; init; }

    [JsonPropertyName("FontFamily")]
    public string? FontFamily { get; init; }

    [JsonPropertyName("WatermarkVersion")]
    public long WatermarkVersion { get; init; }

    [JsonPropertyName("EnableTranscribing")]
    public bool EnableTranscribing { get; init; }

    [JsonPropertyName("EnableTranscribingTitleGeneration")]
    public bool EnableTranscribingTitleGeneration { get; init; }

    [JsonPropertyName("EnableTranscribingDescriptionGeneration")]
    public bool EnableTranscribingDescriptionGeneration { get; init; }

    [JsonPropertyName("EnableTranscribingChaptersGeneration")]
    public bool EnableTranscribingChaptersGeneration { get; init; }

    [JsonPropertyName("EnableTranscribingMomentsGeneration")]
    public bool EnableTranscribingMomentsGeneration { get; init; }

    [JsonPropertyName("TranscribingCaptionLanguages")]
    public List<string>? TranscribingCaptionLanguages { get; init; }

    [JsonPropertyName("EnableCaptionsInPlaylist")]
    public bool? EnableCaptionsInPlaylist { get; init; }

    [JsonPropertyName("RememberPlayerPosition")]
    public bool RememberPlayerPosition { get; init; }

    [JsonPropertyName("EnableMultiAudioTrackSupport")]
    public bool EnableMultiAudioTrackSupport { get; init; }

    [JsonPropertyName("UseSeparateAudioStream")]
    public bool UseSeparateAudioStream { get; init; }

    [JsonPropertyName("JitEncodingEnabled")]
    public bool JitEncodingEnabled { get; init; }

    [JsonPropertyName("EncodingTier")]
    public EncodingTier EncodingTier { get; init; }

    [JsonPropertyName("OutputCodecs")]
    public string? OutputCodecs { get; init; }

    [JsonPropertyName("DrmBasePriceOverride")]
    public decimal? DrmBasePriceOverride { get; init; }

    [JsonPropertyName("DrmCostPerLicenseOverride")]
    public decimal? DrmCostPerLicenseOverride { get; init; }

    [JsonPropertyName("TranscribingPriceOverride")]
    public decimal? TranscribingPriceOverride { get; init; }

    [JsonPropertyName("PremiumEncodingPriceOverride")]
    public decimal? PremiumEncodingPriceOverride { get; init; }

    [JsonPropertyName("MonthlyChargesTranscribing")]
    public decimal MonthlyChargesTranscribing { get; init; }

    [JsonPropertyName("MonthlyChargesPremiumEncoding")]
    public decimal MonthlyChargesPremiumEncoding { get; init; }

    [JsonPropertyName("MonthlyChargesEnterpriseDrm")]
    public decimal MonthlyChargesEnterpriseDrm { get; init; }

    [JsonPropertyName("FeatureFlags")]
    public string? FeatureFlags { get; init; }

    [JsonPropertyName("PlayerVersion")]
    public int PlayerVersion { get; init; }

    [JsonPropertyName("RemoveMetadataFromFallbackVideos")]
    public bool RemoveMetadataFromFallbackVideos { get; init; }

    [JsonPropertyName("ScaleVideoUsingBothDimensions")]
    public bool ScaleVideoUsingBothDimensions { get; init; }

    [JsonPropertyName("ExposeOriginals")]
    public bool ExposeOriginals { get; init; }

    [JsonPropertyName("ExposeVideoMetadata")]
    public bool ExposeVideoMetadata { get; init; }

    [JsonPropertyName("EnableCompactControls")]
    public bool EnableCompactControls { get; init; }

    [JsonPropertyName("GeoBlocking")]
    public VideoGeoBlocking? GeoBlocking { get; init; }
}

public record AppleFairPlayDrm
{
    [JsonPropertyName("Enabled")]
    public bool Enabled { get; init; }

    [JsonPropertyName("CertificateId")]
    public long? CertificateId { get; init; }

    [JsonPropertyName("CertificateExpirationDate")]
    public DateTime? CertificateExpirationDate { get; init; }

    [JsonPropertyName("Provider")]
    public string? Provider { get; init; }
}

public record GoogleWidevineDrm
{
    [JsonPropertyName("Enabled")]
    public bool Enabled { get; init; }

    [JsonPropertyName("CertificateId")]
    public long? CertificateId { get; init; }

    [JsonPropertyName("CertificateExpirationDate")]
    public DateTime? CertificateExpirationDate { get; init; }

    [JsonPropertyName("Provider")]
    public string? Provider { get; init; }

    [JsonPropertyName("SdOnlyForL3")]
    public bool SdOnlyForL3 { get; init; }

    [JsonPropertyName("MinClientSecurityLevel")]
    public WidevineMinClientSecurityLevel? MinClientSecurityLevel { get; init; }
}

public record VideoGeoBlocking
{
    [JsonPropertyName("Enabled")]
    public bool Enabled { get; init; }

    [JsonPropertyName("AllowedCountries")]
    public List<string>? AllowedCountries { get; init; }

    [JsonPropertyName("BlockedCountries")]
    public List<string>? BlockedCountries { get; init; }
}

/// <summary>
/// 0 = Basic, 1 = Enterprise, 2 = BasicV2
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))] // Optional: Remove if you prefer integer serialization
public enum DrmVersion
{
    Basic = 0,
    Enterprise = 1,
    BasicV2 = 2,
}

/// <summary>
/// 0 = Premium, 1 = Volume
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PullZoneType
{
    Premium = 0,
    Volume = 1,
}

/// <summary>
/// 0 = Free, 1 = Premium
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EncodingTier
{
    Free = 0,
    Premium = 1,
}

/// <summary>
/// 0 = None, 1 = L1, 2 = L2, 3 = L3
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WidevineMinClientSecurityLevel
{
    None = 0,
    L1 = 1,
    L2 = 2,
    L3 = 3,
}
