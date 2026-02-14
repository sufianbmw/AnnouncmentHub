using AnnouncmentHub.Models;
using AnnouncmentHub.ViewModels;
using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;

namespace AnnouncmentHub.Service
{
    public class AnnouncementRepository
    {
        private readonly string _connectionString;

        public AnnouncementRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }
        public async Task<AnnouncementSearchResult> GetAnnouncementsDynamic(
    string title,
    //string categoryIdsCsv,
    List<int>? categoryIds,

    int? clientId,
    DateTime? dateFrom,
    DateTime? dateTo,
    int pageNumber,
    int pageSize,
        bool isRandom = false,

    CancellationToken cancellationToken = default)
        {
            title = string.IsNullOrWhiteSpace(title) ? null : title.Trim();

            if (dateTo.HasValue && dateTo.Value.TimeOfDay == TimeSpan.Zero)
                dateTo = dateTo.Value.Date.AddDays(1).AddTicks(-1);

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            var dp = new DynamicParameters();
            dp.Add("Title", title);
            //dp.Add("CategoryIds", categoryIdsCsv);
           // var categoriesTable = BuildIntTable(categoryIds);
            var categoriesTable = BuildIntTable(categoryIds ?? new List<int>());
            dp.Add("CategoryIds", categoriesTable.AsTableValuedParameter("IntList"));


            dp.Add("ClientId", clientId);
            dp.Add("IsActive", dbType: DbType.Boolean, value: (bool?)null); // pass null = no filter (or 1 if you want only active)
            dp.Add("DateFrom", dateFrom);
            dp.Add("DateTo", dateTo);
            dp.Add("PageNumber", pageNumber);
            dp.Add("PageSize", pageSize);
            dp.Add("IsRandom", isRandom); 

            using var multi = await conn.QueryMultipleAsync(
                "SearchAnnouncementsDynamic99",
                dp,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 30
            );

            // 👇 Multi-mapping: AnnouncementDto + Client -> AnnouncementDto
            var announcements =  multi.Read<AnnouncementDto, Client, AnnouncementDto>(
                (a, c) => { a.Client = c; return a; },
                splitOn: "Client_Id" // MUST match the alias in the SP
            ).ToList();

            foreach (var a in announcements)
            {
                if (!string.IsNullOrEmpty(a.CategoriesJson))
                {
                    a.Categories = JsonConvert.DeserializeObject<List<CategoryJsonLink>>(a.CategoriesJson);
                }
                else
                {
                    a.Categories = new List<CategoryJsonLink>();
                }
            }


            var total = await multi.ReadFirstAsync<int>();

            return new AnnouncementSearchResult
            {
                Announcements = announcements,
                TotalCount = total,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        private DataTable BuildIntTable(IEnumerable<int>? ids)
        {
            var dt = new DataTable();
            dt.Columns.Add("Id", typeof(int));

            if (ids != null)
            {
                foreach (var id in ids)
                {
                    dt.Rows.Add(id);
                }
            }

            return dt;
        }


    }
}
