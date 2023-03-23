using GameLauncher_MAUI_CSharp.Code.TorrentLib;
using GameLauncher_MAUI_CSharp.Pages.Elements;
using LiteDB;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

public class Repositories
{
    [BsonId]
    public ObjectId RepId { get; set; }
    public string User { get; set; } = string.Empty;
    public string Rep { get; set; } = string.Empty;
}
public class RepModel
{
    public ObjectId RepId { get; set; } = ObjectId.NewObjectId();
    
    public string NameB
    {
        get
        {
            var db = LauncherApp.db;
            var cl = db.GetCollection<Repositories>("Repositories");
            if (cl.Exists(x => x.RepId == RepId))
            {
                var _repositories = cl.FindOne(x => x.RepId == RepId);
                return _repositories.User;

            }
            else
            {
                return "";
            }

        }
        set
        {
            var db = LauncherApp.db;
            var cl = db.GetCollection<Repositories>("Repositories");
            if (cl.Exists(x => x.RepId == RepId))
            {
                var _repositories = cl.FindOne(x => x.RepId == RepId);
                // _repositories.RepId = ObjectId.NewObjectId();
                _repositories.User = value;
                cl.Update(_repositories);
                return;
            }
            else
            {
                var _repositories = new Repositories
                {
                    User = value,
                    Rep = repB,
                    RepId = RepId
                };
                cl.Insert(_repositories);
                cl.EnsureIndex(x => x.RepId);
            }
        }
    }
    string rep = "";

    public string repB
    {
        get
        {
            var db = LauncherApp.db;
            var cl = db.GetCollection<Repositories>("Repositories");
            if (cl.Exists(x => x.RepId == RepId))
            {
                var _repositories = cl.FindOne(x => x.RepId == RepId);
                rep = _repositories.Rep;
                return rep;

            }
            else
            {
                return rep;
            }

        }
        set
        {
            var db = LauncherApp.db;
            var cl = db.GetCollection<Repositories>("Repositories");
            if (cl.Exists(x => x.RepId == RepId))
            {
                var _repositories = cl.FindOne(x => x.RepId == RepId);
                // _repositories.RepId = ObjectId.NewObjectId();
                _repositories.Rep = value;
                cl.Update(_repositories);
                return;
            }
            else
            {
                var _repositories = new Repositories
                {
                    User = NameB,
                    Rep = value,
                    RepId = RepId
                };
                cl.Insert(_repositories);
                cl.EnsureIndex(x => x.RepId);
            }
        }
    }

}
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public class IsUserNameValidAttribute : ValidationAttribute
{
    public override bool IsValid(object value)
    {
        var inputValue = value as string;
        return TorrentDownloader.UserValid(inputValue);
    }
}
public class Ref<T> where T : struct
{
    public T Value { get; set; }
}
namespace System.ComponentModel.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class IsRepValidAttribute : ValidationAttribute
    {
        [RequiresUnreferencedCode("The property referenced by 'otherProperty' may be trimmed. Ensure it is preserved.")]
        public IsRepValidAttribute(string otherProperty) : base()
        {
            OtherProperty = otherProperty ?? throw new ArgumentNullException(nameof(otherProperty));
        }
        public string OtherProperty { get; }
        public string? OtherPropertyDisplayName { get; internal set; }
        public ValidationResult CisValid(object? value, ValidationContext validationContext) 
        {
          return IsValid(value, validationContext);
        }
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var otherPropertyInfo = validationContext.ObjectType.GetRuntimeProperty(OtherProperty);
            if (otherPropertyInfo == null)
            {
                return new ValidationResult("null");
            }
            if (otherPropertyInfo.GetIndexParameters().Length > 0)
            {
                throw new ArgumentException("Common_PropertyNotFound");
            }
            object? otherPropertyValue = otherPropertyInfo.GetValue(validationContext.ObjectInstance, null);
            var inputValue = value as string;
            if (!TorrentDownloader.RepValid(otherPropertyValue as string, inputValue))
            {
                return new ValidationResult("this user does not have such a repository or does not have access");
            }
            return null;

        }
    }
}

