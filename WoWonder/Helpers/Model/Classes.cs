using System.Collections.Generic;
using Android.Graphics;
using WoWonderClient.Classes.Global;

namespace WoWonder.Helpers.Model
{ 
    public class Classes
    {  
        public class PostType
        {
            public int Id { get; set; }
            public string TypeText { get; set; }
            public int Image { get; set; }
            public string ImageColor { get; set; }
        }
        
        public class Categories
        {
            public string CategoriesId { get; set; }
            public string CategoriesName { get; set; }
            public string CategoriesColor { get; set; }
            public List<SubCategories> SubList { get; set; }
        }

        public class Family
        {
            public string FamilyId { get; set; }
            public string FamilyName { get; set; }
        }

        public class Gender
        {
            public string GenderId { get; set; }
            public string GenderName { get; set; }
            public string GenderColor { get; set; }
            public bool GenderSelect { get; set; }
        }
        
        public class MyInformation
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public Color Color { get; set; }
            public string Icon { get; set; }
            public string Type { get; set; }
        }
          
    }
}