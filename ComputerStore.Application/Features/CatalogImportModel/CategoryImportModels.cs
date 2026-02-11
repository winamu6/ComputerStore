using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerStore.Application.Features.CatalogImportModel
{
    public class CategoryImportModel
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
    }

}
