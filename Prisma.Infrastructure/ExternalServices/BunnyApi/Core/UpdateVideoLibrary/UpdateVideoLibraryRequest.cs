using System.Text.Json.Serialization;
using Prisma.Infrastructure.ExternalServices.BunnyApi.Core.SharedDtos;

namespace Prisma.Infrastructure.ExternalServices.BunnyApi.Core.UpdateVideoLibrary;

/// <summary>
/// Request payload for updating an existing video library.
/// All properties are optional. Only provide the fields you wish to change.
/// </summary>
public sealed record UpdateVideoLibraryRequest
{
    [JsonPropertyName("Name")]
    public string? Name { get; init; }

    [JsonPropertyName("CustomHTML")]
    public string? CustomHTML { get; init; }

    [JsonPropertyName("PlayerKeyColor")]
    public string? PlayerKeyColor { get; init; }

    [JsonPropertyName("EnableTokenAuthentication")]
    public bool? EnableTokenAuthentication { get; init; }

    [JsonPropertyName("EnableTokenIPVerification")]
    public bool? EnableTokenIPVerification { get; init; }

    [JsonPropertyName("ResetToken")]
    public bool? ResetToken { get; init; }

    [JsonPropertyName("WatermarkPositionLeft")]
    public int? WatermarkPositionLeft { get; init; }

    [JsonPropertyName("WatermarkPositionTop")]
    public int? WatermarkPositionTop { get; init; }

    [JsonPropertyName("WatermarkWidth")]
    public int? WatermarkWidth { get; init; }

    [JsonPropertyName("WatermarkHeight")]
    public int? WatermarkHeight { get; init; }

    [JsonPropertyName("EnabledResolutions")]
    public string? EnabledResolutions { get; init; }

    [JsonPropertyName("ViAiPublisherId")]
    public string? ViAiPublisherId { get; init; }

    [JsonPropertyName("VastTagUrl")]
    public string? VastTagUrl { get; init; }

    [JsonPropertyName("WebhookUrl")]
    public string? WebhookUrl { get; init; }

    [JsonPropertyName("CaptionsFontSize")]
    public int? CaptionsFontSize { get; init; }

    [JsonPropertyName("CaptionsFontColor")]
    public string? CaptionsFontColor { get; init; }

    [JsonPropertyName("CaptionsBackground")]
    public string? CaptionsBackground { get; init; }

    [JsonPropertyName("UILanguage")]
    public string? UILanguage { get; init; }

    [JsonPropertyName("AllowEarlyPlay")]
    public bool? AllowEarlyPlay { get; init; }

    [JsonPropertyName("PlayerTokenAuthenticationEnabled")]
    public bool? PlayerTokenAuthenticationEnabled { get; init; }

    [JsonPropertyName("BlockNoneReferrer")]
    public bool? BlockNoneReferrer { get; init; }

    [JsonPropertyName("EnableMP4Fallback")]
    public bool? EnableMP4Fallback { get; init; }

    [JsonPropertyName("KeepOriginalFiles")]
    public bool? KeepOriginalFiles { get; init; }

    [JsonPropertyName("AllowDirectPlay")]
    public bool? AllowDirectPlay { get; init; }

    [JsonPropertyName("EnableDRM")]
    public bool? EnableDRM { get; init; }

    [JsonPropertyName("DrmVersion")]
    public DrmVersion? DrmVersion { get; init; }

    [JsonPropertyName("Controls")]
    public string? Controls { get; init; }

    [JsonPropertyName("PlaybackSpeeds")]
    public string? PlaybackSpeeds { get; init; }

    [JsonPropertyName("Bitrate240p")]
    public int? Bitrate240p { get; init; }

    [JsonPropertyName("Bitrate360p")]
    public int? Bitrate360p { get; init; }

    [JsonPropertyName("Bitrate480p")]
    public int? Bitrate480p { get; init; }

    [JsonPropertyName("Bitrate720p")]
    public int? Bitrate720p { get; init; }

    [JsonPropertyName("Bitrate1080p")]
    public int? Bitrate1080p { get; init; }

    [JsonPropertyName("Bitrate1440p")]
    public int? Bitrate1440p { get; init; }

    [JsonPropertyName("Bitrate2160p")]
    public int? Bitrate2160p { get; init; }

    [JsonPropertyName("ShowHeatmap")]
    public bool? ShowHeatmap { get; init; }

    [JsonPropertyName("EnableContentTagging")]
    public bool? EnableContentTagging { get; init; }

    [JsonPropertyName("FontFamily")]
    public string? FontFamily { get; init; }

    [JsonPropertyName("EnableTranscribing")]
    public bool? EnableTranscribing { get; set; }

    [JsonPropertyName("EnableTranscribingTitleGeneration")]
    public bool? EnableTranscribingTitleGeneration { get; set; }

    [JsonPropertyName("EnableTranscribingDescriptionGeneration")]
    public bool? EnableTranscribingDescriptionGeneration { get; set; }

    [JsonPropertyName("EnableTranscribingChaptersGeneration")]
    public bool? EnableTranscribingChaptersGeneration { get; set; }

    [JsonPropertyName("EnableTranscribingMomentsGeneration")]
    public bool? EnableTranscribingMomentsGeneration { get; set; }

    [JsonPropertyName("TranscribingCaptionLanguages")]
    public List<string>? TranscribingCaptionLanguages { get; set; }

    [JsonPropertyName("EnableCaptionsInPlaylist")]
    public bool? EnableCaptionsInPlaylist { get; set; }

    [JsonPropertyName("RememberPlayerPosition")]
    public bool? RememberPlayerPosition { get; set; }

    [JsonPropertyName("EnableMultiAudioTrackSupport")]
    public bool? EnableMultiAudioTrackSupport { get; set; }

    [JsonPropertyName("UseSeparateAudioStream")]
    public bool? UseSeparateAudioStream { get; set; }

    [JsonPropertyName("JitEncodingEnabled")]
    public bool? JitEncodingEnabled { get; set; }

    [JsonPropertyName("EncodingTier")]
    public EncodingTier? EncodingTier { get; set; }

    [JsonPropertyName("OutputCodecs")]
    public string? OutputCodecs { get; set; }

    [JsonPropertyName("AppleFairPlayDrm")]
    public AppleFairPlayDrmUpdateModel? AppleFairPlayDrm { get; set; }

    [JsonPropertyName("GoogleWidevineDrm")]
    public GoogleWidevineDrmUpdateModel? GoogleWidevineDrm { get; set; }

    [JsonPropertyName("PlayerVersion")]
    public int? PlayerVersion { get; set; }

    [JsonPropertyName("RemoveMetadataFromFallbackVideos")]
    public bool? RemoveMetadataFromFallbackVideos { get; set; }

    [JsonPropertyName("ScaleVideoUsingBothDimensions")]
    public bool? ScaleVideoUsingBothDimensions { get; set; }

    [JsonPropertyName("ExposeOriginals")]
    public bool? ExposeOriginals { get; set; }

    [JsonPropertyName("ExposeVideoMetadata")]
    public bool? ExposeVideoMetadata { get; set; }

    [JsonPropertyName("EnableCompactControls")]
    public bool? EnableCompactControls { get; set; }

    [JsonPropertyName("GeoBlocking")]
    public VideoGeoBlockingUpdateModel? GeoBlocking { get; set; }
}

public class AppleFairPlayDrmUpdateModel
{
    [JsonPropertyName("Enabled")]
    public bool? Enabled { get; set; }
}

public class GoogleWidevineDrmUpdateModel
{
    [JsonPropertyName("Enabled")]
    public bool? Enabled { get; set; }

    [JsonPropertyName("SdOnlyForL3")]
    public bool? SdOnlyForL3 { get; set; }

    [JsonPropertyName("MinClientSecurityLevel")]
    public WidevineMinClientSecurityLevel? MinClientSecurityLevel { get; set; }
}

public class VideoGeoBlockingUpdateModel
{
    [JsonPropertyName("Enabled")]
    public bool? Enabled { get; set; }

    [JsonPropertyName("AllowedCountries")]
    public List<string>? AllowedCountries { get; set; }

    [JsonPropertyName("BlockedCountries")]
    public List<string>? BlockedCountries { get; set; }
}
