using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerStore.Application.Features.CatalogImportModel
{
    public class CatalogImportModel
    {
        public List<CategoryImportModel>? Categories { get; set; }
        public List<ProductImportModel>? Products { get; set; }
    }
}
