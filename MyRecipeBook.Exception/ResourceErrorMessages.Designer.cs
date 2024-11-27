﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MyRecipeBook.Communication {
    using System;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class ResourceErrorMessages {

        private static System.Resources.ResourceManager resourceMan;
        
        private static System.Globalization.CultureInfo resourceCulture;
        
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal ResourceErrorMessages() {
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        public static System.Resources.ResourceManager ResourceManager {
            get {
                if (object.Equals(null, resourceMan)) {
                    System.Resources.ResourceManager temp = new System.Resources.ResourceManager("MyRecipeBook.Exception.ResourceErrorMessages", typeof(ResourceErrorMessages).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        public static System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        public static string NAME_NOT_EMPTY {
            get {
                return ResourceManager.GetString("NAME_NOT_EMPTY", resourceCulture);
            }
        }
        
        public static string EMAIL_NOT_EMPTY {
            get {
                return ResourceManager.GetString("EMAIL_NOT_EMPTY", resourceCulture);
            }
        }
        
        public static string EMAIL_INVALID {
            get {
                return ResourceManager.GetString("EMAIL_INVALID", resourceCulture);
            }
        }
        
        public static string PASSWORD_LENGTH {
            get {
                return ResourceManager.GetString("PASSWORD_LENGTH", resourceCulture);
            }
        }
        
        public static string UNKOWN_ERROR
        {
            get { return ResourceManager.GetString("UNKNOWN_ERROR", resourceCulture);}
        }
        public static string EMAIL_ALREADY_EXISTS
        {
            get { return ResourceManager.GetString("EMAIL_ALREADY_EXISTS", resourceCulture);}
        }
        public static string EMAIL_NOT_REGISTERED
        {
            get { return ResourceManager.GetString("EMAIL_NOT_REGISTERED", resourceCulture);}
        }
        public static string EMAIL_NOT_ACTIVE
        {
            get { return ResourceManager.GetString("EMAIL_NOT_ACTIVE", resourceCulture);}
        }
        public static string PASSWORD_WRONG
        {
            get { return ResourceManager.GetString("PASSWORD_WRONG", resourceCulture);}
        }
        public static string PASSWORD_EMPTY
        {
            get { return ResourceManager.GetString("PASSWORD_EMPTY", resourceCulture);}
        }
        public static string TOKEN_EMPTY
        {
            get { return ResourceManager.GetString("TOKEN_EMPTY", resourceCulture);}
        }
        public static string TOKEN_EXPIRED
        {
            get { return ResourceManager.GetString("TOKEN_EXPIRED", resourceCulture);}
        }
        public static string TOKEN_WITH_NO_PERMISSION
        {
            get { return ResourceManager.GetString("TOKEN_WITH_NO_PERMISSION", resourceCulture);}
        }
        public static string RECIPE_TITLE_EMPTY {
            get {
                return ResourceManager.GetString("RECIPE_TITLE_EMPTY", resourceCulture);
            }
        }
        public static string RECIPE_DIFFICULTY_NOT_IN_ENUM {
            get {
                return ResourceManager.GetString("RECIPE_DIFFICULTY_NOT_IN_ENUM", resourceCulture);
            }
        }

        public static string RECIPE_COOKING_TIME_NOT_IN_ENUM {
            get {
                return ResourceManager.GetString("RECIPE_COOKING_TIME_NOT_IN_ENUM", resourceCulture);
            }
        }

        public static string RECIPE_INGREDIENT_NOT_EMPTY {
            get {
                return ResourceManager.GetString("RECIPE_INGREDIENT_NOT_EMPTY", resourceCulture);
            }
        }

        public static string RECIPE_INSTRUCTION_STEP_GREATER_THAN_0 {
            get {
                return ResourceManager.GetString("RECIPE_INSTRUCTION_STEP_GREATER_THAN_0", resourceCulture);
            }
        }

        public static string RECIPE_INSTRUCTION_TEXT_NOT_EMPTY {
            get {
                return ResourceManager.GetString("RECIPE_INSTRUCTION_TEXT_NOT_EMPTY", resourceCulture);
            }
        }

        public static string RECIPE_INSTRUCTION_TEXT_LESS_THAN_2000 {
            get {
                return ResourceManager.GetString("RECIPE_INSTRUCTION_TEXT_LESS_THAN_2000", resourceCulture);
            }
        }

        public static string RECIPE_INSTRUCTION_DUPLICATE_STEP_INSTRUCTION {
            get {
                return ResourceManager.GetString("RECIPE_INSTRUCTION_DUPLICATE_STEP_INSTRUCTION", resourceCulture);
            }
        }

        public static string RECIPE_DISH_TYPE_NOT_IN_ENUM {
            get {
                return ResourceManager.GetString("RECIPE_DISH_TYPE_NOT_IN_ENUM", resourceCulture);
            }
        }
        public static string RECIPE_AT_LEAST_ONE_INGREDIENT {
            get {
                return ResourceManager.GetString("RECIPE_AT_LEAST_ONE_INGREDIENT", resourceCulture);
            }
        }

        public static string RECIPE_NOT_FOUND
        {
            get {
                return ResourceManager.GetString("RECIPE_NOT_FOUND", resourceCulture);
            }
        }
        public static string RECIPE_NUMBER_GREATER_THAN_0
        {
            get {
                return ResourceManager.GetString("RECIPE_NUMBER_GREATER_THAN_0", resourceCulture);
            }
        }
        public static string RECIPE_INGREDIENT_INVALID_START_CHARACTER
        {
            get {
                return ResourceManager.GetString("RECIPE_INGREDIENT_INVALID_START_CHARACTER", resourceCulture);
            }
        }
        public static string RECIPE_INGREDIENT_MAXIMUM_WORD_COUNT
        {
            get {
                return ResourceManager.GetString("RECIPE_INGREDIENT_MAXIMUM_WORD_COUNT", resourceCulture);
            }
        }
        public static string RECIPE_INGREDIENT_INVALID_CHARACTER
        {
            get {
                return ResourceManager.GetString("RECIPE_INGREDIENT_INVALID_CHARACTER", resourceCulture);
            }
        }
        public static string RECIPE_INGREDIENT_INVALID_SEPARATORS
        {
            get {
                return ResourceManager.GetString("RECIPE_INGREDIENT_INVALID_SEPARATORS", resourceCulture);
            }
        }
        public static string RECIPE_INGREDIENT_UNEXPECTED_ERROR
        {
            get {
                return ResourceManager.GetString("RECIPE_INGREDIENT_UNEXPECTED_ERROR", resourceCulture);
            }
        }
    }
}
