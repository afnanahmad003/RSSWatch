using System.Collections.Generic;
using System.Net;

namespace RESSWATCH.Shopify.Json
{
    #region Post Product To API
    public class APICallResponse
    {
        public string ResponseContent { get; set; } = "";
        public System.Net.HttpStatusCode HTTPStatus { get; set; }
        public bool HasError { get; set; } = false;
        public WebHeaderCollection RequestHeaders { get; set; }
        public WebHeaderCollection ResponseHeaders { get; set; }
        public string PrasedError { get; set; } = "";
        public string PageURLNext { get; set; } = "";
        public string PageURLPrevious { get; set; } = "";
    }
    public class ReviseDescriptionRequest
    {
        public long id { get; set; }
        public string body_html { get; set; }
    }
    public class ReviseImagesRequest
    {
        public long id { get; set; }
        public List<Image> images { get; set; }
        public Image image { get; set; }
    }

    public class Variant
    {
        public string barcode { get; set; }
        public string compare_at_price { get; set; }
        public string created_at { get; set; }
        public string grams { get; set; }
        public string fulfillment_service { get; set; }
        public long id { get; set; }
        public string inventory_management { get; set; }
        public string inventory_policy { get; set; }
        public string option1 { get; set; } = "";
        public string option2 { get; set; } = "";
        public string option3 { get; set; } = "";
        public int position { get; set; }
        public string price { get; set; }
        public long product_id { get; set; }
        public bool requires_shipping { get; set; }
        public string sku { get; set; }
        public string taxable { get; set; }
        public string title { get; set; }
        public string updated_at { get; set; }
        public int inventory_quantity { get; set; }
        public long inventory_item_id { get; set; }
    }

    public class Option
    {
        public long id { get; set; }
        public List<string> values { get; set; }
        public string name { get; set; }
        public int position { get; set; }
        public long product_id { get; set; }
    }

    public class Image
    {
        public List<long> variant_ids { get; set; }
        public string filename { get; set; }
        public string attachment { get; set; }
        public string created_at { get; set; }
        public long id { get; set; }
        public int position { get; set; }
        public long product_id { get; set; }
        public string updated_at { get; set; }
        public string src { get; set; }
        public string alt { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class ProductTagUpdateRequest
    {
        public long id { get; set; }
        public string tags { get; set; }
    }
    public class Product
    {
        public long id { get; set; }
        public string body_html { get; set; }
        public string created_at { get; set; }
        public string handle { get; set; }
        public string product_type { get; set; }
        public string published_at { get; set; }
        public string published_scope { get; set; }
        public object template_suffix { get; set; }
        public string title { get; set; }
        public string updated_at { get; set; }
        public string vendor { get; set; }
        public string tags { get; set; }
        public List<Variant> variants { get; set; }
        public List<Option> options { get; set; }
        public List<Image> images { get; set; }
        public Image image { get; set; }
        public bool? published { get; set; }
        public string metafields_global_title_tag { get; set; }
        public string metafields_global_description_tag { get; set; }
    }

    public class ProductType
    {
        public Product product { get; set; }
        public ErrorsOnProductType errors { get; set; }
    }
    public class ErrorsOnProductType
    {
        public List<string> product { get; set; }
        public List<string> product_tags { get; set; }
    }
    public class ProductsList
    {
        public string errors { get; set; }
        public List<Product> products { get; set; }
    }

    public class GetVariantResponse
    {
        public Variant variant { get; set; }
    }

    public class ReviseMetaKeywordDescriptionAndTitleRequest
    {
        public long id { get; set; }
        public string title { get; set; }
        public string tags { get; set; }
        public string metafields_global_title_tag { get; set; }
        public string metafields_global_description_tag { get; set; }
    }
    public class ReviseTitleRequest
    {
        public long id { get; set; }
        public string title { get; set; }
    }

    #endregion
    #region Scrape Product
    public class ProductDetail
    {
        public string ParentID { get; set; }
        public string ProductName { get; set; }
        public string ProductTitle { get; set; }
        public string ProductPrice { get; set; }
        public string ProductDesc { get; set; }
        public string DeliverDetail { get; set; }
        public string ReturnDetail { get; set; }
        public string Category { get; set; }
        public List<string> Images { get; set; }
        public List<Varient> Variation { get; set; }
    }
    public class Varient
    {
        public string VariationSKU { get; set; }
        public string Option { get; set; }
        public string Qty { get; set; }
    }
    #endregion
}
