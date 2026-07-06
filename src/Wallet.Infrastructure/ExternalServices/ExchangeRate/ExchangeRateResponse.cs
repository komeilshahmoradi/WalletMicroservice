using System.Text.Json.Serialization;

namespace Wallet.Infrastructure.ExternalServices.ExchangeRate;

public sealed record ExchangeRateResponse(
    [property: JsonPropertyName("response")] ExchangeRateResponseData Response
);

public sealed record ExchangeRateResponseData(
    [property: JsonPropertyName("indicators")] IReadOnlyList<IndicatorResponse> Indicators
);

public sealed record IndicatorResponse(
    [property: JsonPropertyName("id")] int Id,

    [property: JsonPropertyName("item_id")] int ItemId,

    [property: JsonPropertyName("market_id")] int MarketId,

    [property: JsonPropertyName("name")] string Name,

    [property: JsonPropertyName("category_id")] string CategoryId,

    [property: JsonPropertyName("title")] string Title,

    [property: JsonPropertyName("slug")] string Slug,

    // قیمت فعلی به صورت رشته (مانند "1760000")
    [property: JsonPropertyName("p")] string Price,

    // بالاترین قیمت روز (High)
    [property: JsonPropertyName("h")] string HighPrice,

    // پایین‌ترین قیمت روز (Low)
    [property: JsonPropertyName("l")] string LowPrice,

    // قیمت باز شدن بازار (Open)
    [property: JsonPropertyName("o")] string OpenPrice,

    // میزان تغییر عددی
    [property: JsonPropertyName("d")] string ChangeAmount,

    // درصد تغییر روزانه
    [property: JsonPropertyName("dp")] double ChangePercent,

    // روند تغییرات (مثلا "high" یا "low")
    [property: JsonPropertyName("dt")] string TrendDirection,

    // تاریخچه متنی قیمت‌ها
    [property: JsonPropertyName("prices")] string PricesHtml,

    // زمان آخرین به روز رسانی به فرمت فارسی/متنی
    [property: JsonPropertyName("t")] string TimeString,

    // تاریخ به روز رسانی به صورت میلادی
    [property: JsonPropertyName("updated_at")] string UpdatedAt
);
