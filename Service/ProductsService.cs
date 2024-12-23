﻿using Data.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WebXeDapAPI.Data;
using WebXeDapAPI.Dto;
using WebXeDapAPI.Models;
using WebXeDapAPI.Models.Enum;
using WebXeDapAPI.Repository.Interface;
using WebXeDapAPI.Service.Interfaces;

namespace WebXeDapAPI.Service
{
    public class ProductsService : IProductsIService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IProductsInterface _productsInterface;
        private readonly IConfiguration _configuration;
        public ProductsService(ApplicationDbContext dbContext, IProductsInterface productsInterface, IConfiguration configuration)
        {
            _configuration = configuration;
            _productsInterface = productsInterface;
            _dbContext = dbContext;
        }
        public async Task<Products> Create(ProductDto productsDto)
        {
            try
            {
                var type = await _dbContext.Types.FindAsync(productsDto.TypeId);
                if (type == null)
                {
                    throw new Exception("typeId not found");
                }

                var brand = await _dbContext.Brands.FindAsync(productsDto.BrandId);
                if (brand == null)
                {
                    throw new Exception("brandId not found");
                }

                // Kiểm tra xem hình ảnh có hợp lệ không
                if (productsDto.image == null || productsDto.image.Length == 0)
                {
                    throw new Exception("Image is required.");
                }

                var imagePath = await SaveImageAsync(productsDto.image);

                Products products = new Products
                {
                    ProductName = productsDto.ProductName,
                    Image = imagePath,
                    Price = productsDto.Price,
                    PriceHasDecreased = productsDto.PriceHasDecreased,
                    Description = productsDto.Description,
                    Quantity = productsDto.Quantity,
                    Create = DateTime.Now,
                    TypeId = type.Id,
                    TypeName = type.Name,
                    BrandId = brand.Id,
                    brandName = brand.BrandName,
                    Colors = productsDto.Colors,
                    Status = StatusProduct.Available,
                    Size = productsDto.Size
                };

                await _dbContext.Products.AddAsync(products);

                // Create stock
                Stock stock = new Stock
                {
                    Product = products,
                    Quantity = 0,
                };

                await _dbContext.Stocks.AddAsync(stock);


                await _dbContext.SaveChangesAsync();
                return products;
            }
            catch (Exception ex)
            {
                // Log chi tiết lỗi
                throw new Exception("There is an error when creating a Product", ex);
            }
        }

        public async Task<bool> DeleteAsync(int Id)
        {
            try
            {
                var delete = await _dbContext.Products.FirstOrDefaultAsync(x => x.Id == Id);
                if (delete == null)
                {
                    throw new Exception("Id not found");
                }
                _dbContext.Products.Remove(delete);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("There was an error while deleting the Product", ex);
            }
        }

        public List<object> GetBrandName(string keyword)
        {
            keyword = keyword.ToLower();
            List<object> result = new List<object>();

            List<ProductBrandInfDto> products = _productsInterface.GetAllBrandName(keyword);
            foreach (var item in products)
            {
                var productBrandInfo = new ProductBrandInfDto
                {
                    ProductName = item.ProductName,
                    Price = item.Price,
                    PriceHasDecreased = item.PriceHasDecreased,
                    Description = item.Description,
                    Image = item.Image,
                    BrandName = item.BrandName,
                };
                result.Add(productBrandInfo);
            }
            return result;
        }

        public List<object> GetPriceHasDecreased()
        {
            List<object> result = new List<object>();

            List<ProductPriceHasDecreasedInfDto> products = _productsInterface.SearchProductsByPriceHasDecreased();
            foreach (var item in products)
            {
                var productPriceHasDecreasedInfDto = new ProductPriceHasDecreasedInfDto
                {
                    Id = item.Id,
                    ProductName = item.ProductName,
                    Price = item.Price,
                    PriceHasDecreased = item.PriceHasDecreased,
                    Description = item.Description,
                    Image = item.Image
                };
                result.Add(productPriceHasDecreasedInfDto);
            }
            return result;
        }

        public List<object> GetTypeName(string keyword, int limit = 8)
        {
            List<object> resultType = new List<object>();
            List<ProductTypeInfDto> products = _productsInterface.GetAllTypeName(keyword);
            for (int i = 0; i < Math.Min(limit, products.Count); i++)
            {
                var item = products[i];
                var productTypeInfo = new ProductTypeInfDto
                {
                    Id = item.Id,
                    ProductName = item.ProductName,
                    Price = item.Price,
                    PriceHasDecreased = item.PriceHasDecreased,
                    Description = item.Description,
                    Image = item.Image,
                    TypeName = item.TypeName,
                    Colors = item.Colors,
                };
                resultType.Add(productTypeInfo);
            }
            return resultType;
        }

        private async Task<string> SaveImageAsync(IFormFile image)
        {
            try
            {
                string currentDataFolder = DateTime.Now.ToString("dd-MM-yyyy");
                var baseFolder = _configuration.GetValue<string>("BaseAddress");

                // Tạo thư mục Product
                var productFolder = Path.Combine(baseFolder, "Product");

                if (!Directory.Exists(productFolder))
                {
                    Directory.CreateDirectory(productFolder);
                }

                // Tạo thư mục date nằm trong thư mục Product
                var folderPath = Path.Combine(productFolder, currentDataFolder);

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                string filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream); // Lưu hình ảnh vào file
                }

                // Trả về tên thư mục và tên ảnh
                return Path.Combine("Product", currentDataFolder, fileName);
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while saving the image: {ex.Message}");
            }
        }

        public byte[] GetProductImageBytes(string image)
        {
            try
            {
                if (string.IsNullOrEmpty(image))
                {
                    throw new FileNotFoundException("Image not found!");
                }

                var baseFolder = _configuration.GetValue<string>("BaseAddress");
                var fullPath = Path.Combine(baseFolder, image); // Tạo đường dẫn tuyệt đối

                if (!File.Exists(fullPath))
                {
                    throw new FileNotFoundException("Image not found!");
                }

                return File.ReadAllBytes(fullPath);
            }
            catch (Exception e)
            {
                throw new Exception($"An error occurred: {e.Message}");
            }
        }

        public async Task<UpdateProductDto> Update(int Id, UpdateProductDto updateProductDto)
        {
            try
            {
                var product = await _dbContext.Products.FirstOrDefaultAsync(x => x.Id == Id);
                if (product == null)
                {
                    throw new Exception("ProductId not found");
                }

                if (!string.IsNullOrEmpty(updateProductDto.ProductName) && updateProductDto.ProductName != "null")
                {
                    product.ProductName = updateProductDto.ProductName;
                }

                if (updateProductDto.Price > 0)
                {
                    product.Price = updateProductDto.Price;
                }

                if (updateProductDto.Size != null)
                {

                    product.Size = updateProductDto.Size;
                }

                if (updateProductDto.PriceHasDecreased > 0)
                {
                    product.PriceHasDecreased = updateProductDto.PriceHasDecreased;
                }

                if (!string.IsNullOrEmpty(updateProductDto.Description) && updateProductDto.Description != "null")
                {
                    product.Description = updateProductDto.Description;
                }

                if (updateProductDto.Quantity >= 0)
                {
                    product.Quantity = updateProductDto.Quantity;
                }

                if (updateProductDto.Image != null)
                {
                    product.Image = await SaveImageAsync(updateProductDto.Image);
                }

                if (updateProductDto.Create.HasValue)
                {
                    product.Create = updateProductDto.Create.Value;
                }
                product.Status = updateProductDto.Status;
                await _dbContext.SaveChangesAsync();
                return updateProductDto;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while updating the product: {ex.Message}");
            }
        }

        public List<ProductGetAllInfPriceDto> GetProductsWithinPriceRangeAndBrand(string productType, decimal minPrice, decimal maxPrice, string? brandsName)
        {
            // Giả sử _context là DbContext chứa các DbSet cho Product và Type
            var products = (from p in _dbContext.Products
                            join t in _dbContext.Types on p.TypeId equals t.Id
                            where t.Name == productType
                            select new ProductGetAllInfPriceDto
                            {
                                Id = p.Id,
                                ProductName = p.ProductName,
                                Price = p.Price,
                                PriceHasDecreased = p.PriceHasDecreased,
                                Image = p.Image,
                                BrandNamer = p.brandName
                            }).ToList();

            // Lọc thêm theo khoảng giá và thương hiệu
            var result = products
                .Where(p => p.Price >= minPrice && p.Price <= maxPrice
                             && (string.IsNullOrEmpty(brandsName) || p.BrandNamer.Equals(brandsName, StringComparison.OrdinalIgnoreCase)))
                .GroupBy(p => p.ProductName)  // Nhóm theo ProductName
                .Select(group => group.First())  // Chọn sản phẩm đầu tiên trong mỗi nhóm (loại bỏ trùng)
                .ToList();

            return result;
        }

        public ProductDetailWithColors GetProductsByNameAndColor(string productName, string? color, string? size)
        {
            var result = new ProductDetailWithColors();
            var productsQuery = _dbContext.Products.Where(p => p.ProductName == productName);

            if (!productsQuery.Any())
            {
                throw new Exception("Không tìm thấy sản phẩm nào với thông tin cung cấp.");
            }

            var filteredProductQuery = productsQuery;

            if (!string.IsNullOrEmpty(color))
            {
                filteredProductQuery = filteredProductQuery.Where(p => p.Colors == color);
            }
            if (!string.IsNullOrEmpty(size))
            {
                filteredProductQuery = filteredProductQuery.Where(p => p.Size == size);
            }

            var productWithSpecificAttributes = filteredProductQuery.FirstOrDefault();

            if (productWithSpecificAttributes != null)
            {
                result.ProductDetail = MapToProductDetail(productWithSpecificAttributes);
            }
            else
            {
                result.ProductDetails = productsQuery
                    .Select(p => MapToProductDetail(p))
                    .ToList();
            }

            result.AvailableColors = productsQuery.Select(p => p.Colors).Distinct().ToList();
            result.AvailableSize = productsQuery.Select(p => p.Size).Distinct().ToList();

            return result;
        }

        // Update the parameter type to match the correct model class name (Products)
        private Product_detail MapToProductDetail(Products product)
        {
            return new Product_detail
            {
                Id = product.Id,
                ProductName = product.ProductName,
                Price = product.Price,
                PriceHasDecreased = product.PriceHasDecreased,
                Description = product.Description,
                Image = product.Image,
                brandName = product.brandName,
                TypeName = product.TypeName,
                Colors = product.Colors,
                Size = product.Size,
                Status = ((StatusProduct)product.Status).ToString()
            };
        }


        public async Task<List<GetViewProductType>> GetProductType(string productType)
        {
            try
            {
                var type = await _dbContext.Types.FirstOrDefaultAsync(x => x.Description == productType);
                if (type == null)
                {
                    throw new Exception("TypeName not found");
                }

                var typeProduct = await (from t in _dbContext.Types
                                         join p in _dbContext.Products
                                         on t.Id equals p.TypeId
                                         where t.Description == productType
                                         select new GetViewProductType
                                         {
                                             Id = p.Id,
                                             ProductName = p.ProductName,
                                             Price = p.Price,
                                             PriceHasDecreased = p.PriceHasDecreased,
                                             Description = p.Description,
                                             Image = p.Image,
                                             BrandId = p.BrandId,
                                             brandName = p.brandName,
                                             TypeId = p.TypeId,
                                             TypeName = p.TypeName,
                                             Colors = p.Colors,
                                             ProductType_ProductType = t.ProductType
                                         })
                                         .GroupBy(p => p.ProductName) // Nhóm theo ProductName
                                         .Select(g => g.First()) // Lấy sản phẩm đầu tiên trong mỗi nhóm
                                         .ToListAsync();

                return typeProduct;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching the product type", ex);
            }
        }


        public async Task<List<ProductTypeInfDto>> GetProductName(string productName)
        {
            try
            {
                var products = await _dbContext.Products
                    .Where(x => x.ProductName.ToLower().Contains(productName.ToLower()))
                    .Select(product => new ProductTypeInfDto
                    {
                        Id = product.Id,
                        ProductName = product.ProductName,
                        Price = product.Price,
                        PriceHasDecreased = product.PriceHasDecreased,
                        Description = product.Description,
                        Image = product.Image,
                        TypeName = product.TypeName,
                        Colors = product.Colors,
                    }).ToListAsync();

                if (products.Count == 0)
                {
                    return new List<ProductTypeInfDto>();
                }

                return products;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while fetching the products with name '{productName}'", ex);
            }
        }

        public List<ProductGetAllInfDto> GetAllProduct()
        {
            var products = _dbContext.Products
                .Select(product => new ProductGetAllInfDto
                {
                    Id = product.Id,
                    ProductName = product.ProductName,
                    Price = product.Price,
                    PriceHasDecreased = product.PriceHasDecreased,
                    Description = product.Description,
                    Quantity = product.Quantity,
                    Image = null,
                    Create = product.Create,
                    Status = product.Status.ToString()
                }).
                ToList();
            return products;
        }

        public async Task<List<GetViewProductType>> GetBoSuuTap(string productType)
        {
            try
            {
                var type = await _dbContext.Types.FirstOrDefaultAsync(x => x.ProductType == productType);
                if (type == null)
                {
                    throw new Exception("TypeName not found");
                }

                var typeProduct = await (from t in _dbContext.Types
                                         join p in _dbContext.Products
                                         on t.Id equals p.TypeId
                                         where t.ProductType == productType
                                         select new GetViewProductType
                                         {
                                             Id = p.Id,
                                             ProductName = p.ProductName,
                                             Price = p.Price,
                                             PriceHasDecreased = p.PriceHasDecreased,
                                             Description = p.Description,
                                             Image = p.Image,
                                             BrandId = p.BrandId,
                                             brandName = p.brandName,
                                             TypeId = p.TypeId,
                                             TypeName = p.TypeName,
                                             Colors = p.Colors,
                                             ProductType_ProductType = t.ProductType
                                         })
                                         .GroupBy(p => p.ProductName) // Group by ProductName to remove duplicates
                                         .Select(g => g.First()) // Select only the first item from each group
                                         .ToListAsync();

                return typeProduct;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching the product type", ex);
            }
        }

        public async Task<List<productsSearchKey>> SearchKey(string keyWord)
        {
            try
            {
                var products = await _dbContext.Products
                    .Where(p => p.ProductName.Contains(keyWord) || p.brandName.Contains(keyWord) || p.TypeName.Contains(keyWord))
                    .ToListAsync();  // Lấy toàn bộ sản phẩm thỏa mãn điều kiện từ cơ sở dữ liệu

                // Nhóm theo tên sản phẩm và chọn sản phẩm đầu tiên trong mỗi nhóm
                var distinctProducts = products
                    .GroupBy(p => p.ProductName)
                    .Select(g => g.First())  // Lấy sản phẩm đầu tiên trong mỗi nhóm (tên duy nhất)
                    .Select(p => new productsSearchKey
                    {
                        Id = p.Id,
                        ProductName = p.ProductName,
                        Price = p.Price,
                        PriceHasDecreased = p.PriceHasDecreased,
                        Description = p.Description,
                        Image = p.Image,
                        brandName = p.brandName,
                        TypeName = p.TypeName,
                        Colors = p.Colors,
                        Size = p.Size,
                        Status = p.Status.ToString()
                    })
                    .ToList();  // Chuyển nhóm về danh sách

                return distinctProducts;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi tìm kiếm sản phẩm.", ex);
            }
        }
    }
}