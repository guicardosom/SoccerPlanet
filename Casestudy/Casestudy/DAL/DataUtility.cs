using Casestudy.DAL.DomainClasses;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Casestudy.DAL
{
    public class DataUtility
    {
        private readonly AppDbContext _db;
        public DataUtility(AppDbContext context)
        {
            _db = context;
        }

        public async Task<bool> LoadProductInfoFromJsonToDb(string stringJson)
        {
            bool brandsLoaded = false;
            bool productsLoaded = false;

            try
            {
                // an element that is typed as dynamic is assumed to support any operation
                dynamic? objectJson = JsonSerializer.Deserialize<Object>(stringJson);
                brandsLoaded = await LoadBrands(objectJson);
                productsLoaded = await LoadProducts(objectJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return brandsLoaded && productsLoaded;
        }

        private async Task<bool> LoadBrands(dynamic jsonObjectArray)
        {
            bool loadedBrands = false;
            try
            {
                // clear out the old rows
                _db.Brands?.RemoveRange(_db.Brands);
                await _db.SaveChangesAsync();

                List<String> allBrands = new();
                foreach (JsonElement element in jsonObjectArray.EnumerateArray())
                {
                    if (element.TryGetProperty("BRAND", out JsonElement productJson))
                    {
                        allBrands.Add(productJson.GetString()!);
                    }
                }

                IEnumerable<String> brands = allBrands.Distinct<String>();
                foreach (string brandName in brands)
                {
                    Brand brand = new();
                    brand.Name = brandName;
                    await _db.Brands!.AddAsync(brand);
                    await _db.SaveChangesAsync();
                }

                loadedBrands = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error - " + ex.Message);
            }

            return loadedBrands;
        }

        private async Task<bool> LoadProducts(dynamic jsonObjectArray)
        {
            bool loadedProducts = false;
            try
            {
                List<Brand> brands = _db.Brands!.ToList();

                // clear out the old data
                _db.Products?.RemoveRange(_db.Products);
                await _db.SaveChangesAsync();

                foreach (JsonElement element in jsonObjectArray.EnumerateArray())
                {
                    Product item = new();
                    item.Id = GenerateProductId(element.GetProperty("PRODUCTNAME").GetString()!, element.GetProperty("BRAND").GetString()!);
                    item.ProductName = element.GetProperty("PRODUCTNAME").GetString();
                    item.GraphicName = element.GetProperty("GRAPHICNAME").GetString();
                    item.CostPrice = Convert.ToDecimal(element.GetProperty("COST").GetString());
                    item.MSRP = Convert.ToDecimal(element.GetProperty("MSRP").GetString());
                    item.QtyOnHand = Convert.ToInt32(element.GetProperty("QTYHAND").GetString());
                    item.QtyOnBackOrder = Convert.ToInt32(element.GetProperty("QTYORDER").GetString());
                    item.Description = element.GetProperty("DESC").GetString();
                    string? prodBrand = element.GetProperty("BRAND").GetString();

                    // add the FK here
                    foreach (Brand brand in brands)
                    {
                        if (brand.Name == prodBrand)
                        {
                            item.Brand = brand;
                            break;
                        }
                    }

                    await _db.Products!.AddAsync(item);
                    await _db.SaveChangesAsync();
                }

                loadedProducts = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error - " + ex.Message);
            }

            return loadedProducts;
        }

        //Utilizes the MD5 hashing algorithm on a combined string of ProductName and BrandName.
        //Finally, we convert the resulting hash into a 15-character ID by taking pairs of bytes,
        //summing them, and converting the resulting value into a character.
        private string GenerateProductId(string productName, string brandName)
        {
            string combinedString = $"{productName}_{brandName}";

            using (var md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(combinedString));

                StringBuilder sb = new StringBuilder(30);
                for (int i = 0; i < 15; i++)
                {
                    int value = hashBytes[i] % 36;
                    char character = (char)(value < 10 ? '0' + value : 'A' + value - 10);
                    sb.Append(character);
                }

                return sb.ToString();
            }
        }
    }
}
