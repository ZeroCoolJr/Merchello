﻿namespace Merchello.Web.Editors.Reports
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using Merchello.Core;
    using Merchello.Core.Services;
    using Merchello.Web.Models.Querying;
    using Merchello.Web.Models.Reports;
    using Merchello.Web.Reporting;
    using Merchello.Web.Trees;

    using Umbraco.Core;
    using Umbraco.Core.Services;
    using Umbraco.Web;
    using Umbraco.Web.Mvc;

    /// <summary>
    /// A controller for rendering the sales over time report.
    /// </summary>
    [PluginController("Merchello")]
    public class SalesOverTimeReportApiController : ReportController
    {
        /// <summary>
        /// The <see cref="CultureInfo"/>.
        /// </summary>
        private readonly CultureInfo _culture;

        /// <summary>
        /// The <see cref="IInvoiceService"/>.
        /// </summary>
        private readonly IInvoiceService _invoiceService;

        /// <summary>
        /// The text service.
        /// </summary>
        private readonly ILocalizedTextService _textService;



        /// <summary>
        /// Initializes a new instance of the <see cref="SalesOverTimeReportApiController"/> class.
        /// </summary>
        public SalesOverTimeReportApiController()
            : this(Core.MerchelloContext.Current)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SalesOverTimeReportApiController"/> class.
        /// </summary>
        /// <param name="merchelloContext">
        /// The merchello context.
        /// </param>
        public SalesOverTimeReportApiController(IMerchelloContext merchelloContext)
            : this(merchelloContext, UmbracoContext.Current)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SalesOverTimeReportApiController"/> class.
        /// </summary>
        /// <param name="merchelloContext">
        /// The <see cref="IMerchelloContext"/>.
        /// </param>
        /// <param name="umbracoContext">
        /// The umbraco Context.
        /// </param>
        public SalesOverTimeReportApiController(IMerchelloContext merchelloContext, UmbracoContext umbracoContext)
            : base(merchelloContext)
        {
            _culture = LocalizationHelper.GetCultureFromUser(umbracoContext.Security.CurrentUser);

            _invoiceService = merchelloContext.Services.InvoiceService;

            _textService = umbracoContext.Application.Services.TextService;
        }

        /// <summary>
        /// Gets the base url definition for Server Variables Parsing.
        /// </summary>
        public override KeyValuePair<string, object> BaseUrl
        {
            get
            {
                return GetBaseUrl<SalesOverTimeReportApiController>("merchelloSalesOverTimeApiBaseUrl");
            }
        }

        /// <summary>
        /// Gets the default report data for initial page load.
        /// </summary>
        /// <returns>
        /// The <see cref="QueryResultDisplay"/>.
        /// </returns>
        public override QueryResultDisplay GetDefaultReportData()
        {
            var today = DateTime.Today;
            var endOfMonth = GetEndOfMonth(today);
            var startMonth = endOfMonth.AddMonths(-11);
            var startOfYear = GetFirstOfMonth(startMonth);

            return BuildResult(startOfYear, endOfMonth);
        }

        /// <summary>
        /// Gets the first day a month month.
        /// </summary>
        /// <param name="current">
        /// The reference date.
        /// </param>
        /// <returns>
        /// The <see cref="DateTime"/>.
        /// </returns>
        private static DateTime GetFirstOfMonth(DateTime current)
        {
            return new DateTime(current.Year, current.Month, 1);
        }

        /// <summary>
        /// Gets the last day of a month.
        /// </summary>
        /// <param name="current">
        /// The reference date.
        /// </param>
        /// <returns>
        /// The <see cref="DateTime"/>.
        /// </returns>
        private static DateTime GetEndOfMonth(DateTime current)
        {
            return new DateTime(current.Year, current.Month, DateTime.DaysInMonth(current.Year, current.Month));
        }

        private QueryResultDisplay BuildResult(DateTime startDate, DateTime endDate)
        {
            var count = 0;

            var currentDate = startDate;
            var results = new List<SalesOverTimeResult>();

            while (currentDate <= endDate)
            {
                currentDate = startDate.AddMonths(1);
                count++;
                results.Add(GetResult(startDate, currentDate));
                startDate = currentDate;
            }

            return new QueryResultDisplay()
                       {
                           Items = results,
                           CurrentPage = 1,
                           ItemsPerPage = count,
                           TotalItems = count,
                           TotalPages = 1
                       };
        }

        /// <summary>
        /// Gets the sales result.
        /// </summary>
        /// <param name="startDate">
        /// The start date.
        /// </param>
        /// <param name="endDate">
        /// The end date.
        /// </param>
        /// <returns>
        /// The <see cref="SalesOverTimeResult"/>.
        /// </returns>
        private SalesOverTimeResult GetResult(DateTime startDate, DateTime endDate)
        {
            var monthName = _textService.GetLocalizedMonthName(_culture, startDate.Month);

            var count = _invoiceService.CountInvoices(startDate, endDate);

            return new SalesOverTimeResult() { Month = monthName, Year = startDate.Year.ToString(), SalesCount = count };
        }

    }
}