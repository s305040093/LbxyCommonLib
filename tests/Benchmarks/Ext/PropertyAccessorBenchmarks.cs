using BenchmarkDotNet.Attributes;
using LbxyCommonLib.Ext;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Benchmarks.Ext
{
    public class TestModel
    {
        [DisplayName("Name_Display")]
        public string Name { get; set; } = "DefaultName";

        [Display(Name = "Age_Display")]
        public int Age { get; set; } = 18;

        public DateTime Date { get; set; } = DateTime.Now;
    }

    [MemoryDiagnoser]
    public class PropertyAccessorBenchmarks
    {
        private readonly TestModel _model = new TestModel();
        private readonly PropertyInfo _nameProp;
        private readonly PropertyInfo _ageProp;

        public PropertyAccessorBenchmarks()
        {
            _nameProp = typeof(TestModel).GetProperty("Name");
            _ageProp = typeof(TestModel).GetProperty("Age");

            // Warm up cache
            PropertyAccessor.GetProperties<TestModel>();
        }

        [Benchmark(Baseline = true)]
        public string GetDisplayName_Reflection()
        {
            var attr = _nameProp.GetCustomAttribute<DisplayNameAttribute>();
            return attr?.DisplayName ?? _nameProp.Name;
        }

        [Benchmark]
        public string GetDisplayName_PropertyAccessor()
        {
            return PropertyAccessor.GetDisplayName<TestModel>("Name");
        }

        [Benchmark]
        public object GetValue_Reflection()
        {
            return _nameProp.GetValue(_model);
        }

        [Benchmark]
        public object GetValue_PropertyAccessor()
        {
            return PropertyAccessor.GetValue(_model, "Name");
        }

        [Benchmark]
        public object GetValue_PropertyAccessor_DisplayName()
        {
            return PropertyAccessor.GetValue(_model, "Name_Display", useDisplayName: true);
        }

        [Benchmark]
        public void SetValue_PropertyAccessor_DisplayName()
        {
            PropertyAccessor.SetValue(_model, "Name_Display", "NewName", useDisplayName: true);
        }

        [Benchmark]
        public Dictionary<string, string> ToPropertyDictionary_PropertyAccessor()
        {
            return _model.ToPropertyDictionary();
        }

        // Reflection based implementation of ToPropertyDictionary for comparison
        [Benchmark]
        public Dictionary<string, string> ToPropertyDictionary_Reflection()
        {
            var target = new Dictionary<string, string>();
            foreach (var prop in typeof(TestModel).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.GetIndexParameters().Length > 0) continue;
                var val = prop.GetValue(_model);
                target[prop.Name] = val?.ToString() ?? "";
            }
            return target;
        }
    }
}
