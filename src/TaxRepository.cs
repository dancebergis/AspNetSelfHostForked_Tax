﻿using System;
using System.Collections.Generic;
using System.Globalization;
using AspNetSelfHostDemo.Entities;

namespace AspNetSelfHostDemo
{
    public interface ITaxRepository
    {
        decimal GetTax(string city, DateTime date);
        void AddYearlyTax(string city, int year, decimal tax);
        void AddMonthlyTax(string city, int year, int month, decimal tax);
        void AddDailyTax(string city, DateTime day, decimal tax);
        void AddWeeklyTax(string city, int year, int weekOfYear, decimal tax);
    }

    public class TaxRepository : ITaxRepository
    {
        private const decimal NoTax = 0m;
        private readonly Dictionary<string, CityTax> _cityTaxesCache;

        public TaxRepository()
        {
            _cityTaxesCache = new Dictionary<string, CityTax>();
        }

        public void AddYearlyTax(string city, int year, decimal tax)
        {
            var yearlyTax = GetYearlyTaxesOrInit(city, year);
            yearlyTax.YearlyTax = tax;
        }

        public void AddMonthlyTax(string city, int year, int month, decimal tax)
        {
            var yearlyTax = GetYearlyTaxesOrInit(city, year);

            decimal monthlyTax;
            if (yearlyTax.MonthTaxes.TryGetValue(month, out monthlyTax))
            {
                yearlyTax.MonthTaxes.Remove(month);
            }
            yearlyTax.MonthTaxes.Add(month, tax);
        }

        public void AddDailyTax(string city, DateTime day, decimal tax)
        {
            var yearlyTax = GetYearlyTaxesOrInit(city, day.Year);

            decimal daylyTax;
            if (yearlyTax.DayTaxes.TryGetValue(day.DayOfYear, out daylyTax))
            {
                yearlyTax.DayTaxes.Remove(day.DayOfYear);
            }
            yearlyTax.DayTaxes.Add(day.DayOfYear, tax);
        }

        public void AddWeeklyTax(string city, int year, int weekOfYear, decimal tax)
        {
            var yearlyTax = GetYearlyTaxesOrInit(city, year);

            decimal weeklyTax;
            if (yearlyTax.WeekTaxes.TryGetValue(weekOfYear, out weeklyTax))
            {
                yearlyTax.WeekTaxes.Remove(weekOfYear);
            }
            yearlyTax.WeekTaxes.Add(weekOfYear, tax);
        }

        public decimal GetTax(string city, DateTime date)
        {
            CityTax cityTaxes;
            if (_cityTaxesCache.TryGetValue(city, out cityTaxes) == false)
            {
                return NoTax;
            }

            YearTax cityTax;
            if (!cityTaxes.YearTaxes.TryGetValue(date.Year, out cityTax)) return NoTax;

            decimal dailyTax, weeklyTax, monthlyTax;
            if (cityTax.DayTaxes.TryGetValue(date.DayOfYear, out dailyTax))
            {
                return dailyTax;
            }

            // fallback to Weekly Taxes
            var weekOfYear = CalendarHelper.GetWeekOfYear(date);
            if (cityTax.WeekTaxes.TryGetValue(weekOfYear, out weeklyTax))
            {
                return weeklyTax;
            }

            // fallback to Montly and Yearly Taxes
            return cityTax.MonthTaxes.TryGetValue(date.Month, out monthlyTax) ? monthlyTax : cityTax.YearlyTax;
        }

        private CityTax GetCityTaxes(string city)
        {
            CityTax cityTaxes;
            _cityTaxesCache.TryGetValue(city, out cityTaxes);
            if (cityTaxes == null)
            {
                cityTaxes = new CityTax();
                _cityTaxesCache.Add(city, cityTaxes);
            }
            return cityTaxes;
        }

        private YearTax GetYearlyTaxesOrInit(string city, int year, decimal tax = NoTax)
        {
            var cityTaxes = GetCityTaxes(city);

            YearTax yearlyTax;
            if (cityTaxes.YearTaxes.TryGetValue(year, out yearlyTax))
            {
                return yearlyTax;
            }
            yearlyTax = new YearTax() {YearlyTax = tax};
            cityTaxes.YearTaxes.Add(year, yearlyTax);
            return yearlyTax;
        }

    }
}
