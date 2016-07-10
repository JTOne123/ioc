﻿using System;
using System.Linq;
using System.Collections.Generic;

namespace nutility
{
  /// <summary>
  /// Given the runtime dependency management tradition of early design patterns, e.g., Microsoft COM IUnknown::QueryInterface method, this class follows
  /// such design tradition and relies on basic equivalent mechanisms from .NET Framework (System.IServiceProvider interface).
  /// </summary>
  public class TypeClassMapper : ITypeClassMapper
  {
    /// <summary>
    /// Keep basic Type - Class mapping.
    /// </summary>
    private IDictionary<string, object> typemap;

    IDictionary<string, Func<object>> typecreatormap;
    
    /// <summary>
    /// Name of the configured Scope.
    /// </summary>
    private string scope;

    /// <summary>
    /// Name of the section in configuration file.
    /// </summary>
    private string section;

    /// <summary>
    /// For implicit, config-based, type-class mappings.
    /// </summary>
    /// <param name="scope">Name of the configured Scope. Default means required <see cref="MappingCollection"/> configured element.</param>
    /// <param name="section">Name of the section in configuration file. Default means the <see cref="TypeClassMapperConfigurationSection"/> class name.</param>
    public TypeClassMapper(string scope = null, string section = null)
    {
      this.scope = scope;
      this.section = section;
      TypeClassMapperConfigurationSection configSection = GetConfiguration(section);
      InitAndLoadMappings(configSection, scope);
      this.typecreatormap = new Dictionary<string, Func<object>>();
    }

    /// <summary>
    /// For explicit type-class mappings.
    /// </summary>
    /// <param name="typeclassmap">Type-Class map</param>
    public TypeClassMapper(IDictionary<string, object> typeclassmap)
    {
      if (typeclassmap == null)
      {
        throw new TypeClassMapperException($"Parameter cannot be null: {nameof(typeclassmap)}", new ArgumentNullException(nameof(typeclassmap)));
      }
      this.scope = "<explicit>";
      this.section = "<explicit>";
      this.typemap = typeclassmap;
      this.typecreatormap = new Dictionary<string, Func<object>>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="typeclassmap"></param>
    public TypeClassMapper(IDictionary<Type, object> typeclassmap)
    {
      if (typeclassmap == null)
      {
        throw new TypeClassMapperException($"Parameter cannot be null: {nameof(typeclassmap)}", new ArgumentNullException(nameof(typeclassmap)));
      }
      this.scope = "<explicit>";
      this.section = "<explicit>";
      this.typemap = typeclassmap.Aggregate(new Dictionary<string, object>(), (whole, next) => { whole.Add(next.Key.FullName, next.Value); return whole; });
      this.typecreatormap = new Dictionary<string, Func<object>>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="typeclassmap"></param>
    public TypeClassMapper(IDictionary<Type, Type> typeclassmap)
    {
      if (typeclassmap == null)
      {
        throw new TypeClassMapperException($"Parameter cannot be null: {nameof(typeclassmap)}", new ArgumentNullException(nameof(typeclassmap)));
      }
      this.scope = "<explicit>";
      this.section = "<explicit>";
      this.typemap = typeclassmap.Aggregate(new Dictionary<string, object>(), (whole, next) => { whole.Add(next.Key.FullName, next.Value.AssemblyQualifiedName); return whole; });
      this.typecreatormap = new Dictionary<string, Func<object>>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="typeclassmap"></param>
    /// <param name="typecreatormap"></param>
    public TypeClassMapper(IDictionary<Type, Type> typeclassmap, IDictionary<Type, Func<object>> typecreatormap) : this(typeclassmap)
    {
      this.typecreatormap = typecreatormap.Aggregate(new Dictionary<string, Func<object>>(), (whole, next) => { whole.Add(next.Key.FullName, next.Value); return whole; });
    }

    /// <summary>
    /// Existing type-class mappings.
    /// </summary>
    public IEnumerable<KeyValuePair<string, object>> Mappings { get { return typemap; } }

    /// <summary>
    /// Given a Type, it returns its configured/mapped Class.
    /// </summary>
    /// <param name="requiredType">Required Type</param>
    /// <returns>Mapped Class</returns>
    public virtual object GetService(Type requiredType)
    {
      if (requiredType == null)
      {
        throw new TypeClassMapperException($"Parameter cannot be null: {nameof(requiredType)}", new ArgumentNullException(nameof(requiredType)));
      }
      object mapped_value = null;
      if (typecreatormap.ContainsKey(requiredType.FullName))
      {
        if (typecreatormap[requiredType.FullName] != null)
        {
          mapped_value = typecreatormap[requiredType.FullName]();
        }
      }
      else
      {
        if (!typemap.ContainsKey(requiredType.FullName))
        {
          throw new TypeClassMapperException($"Type not found: [{requiredType.FullName}] at configured scope [{scope ?? "<default>"}] and section [{section ?? "<default>"}]");
        }
        mapped_value = typemap[requiredType.FullName];
        if (mapped_value is string)
        {
          mapped_value = CreateInstanceOfMappedClass(mapped_value as string, requiredType);
        }
      }
      return mapped_value;
    }

    /// <summary>
    /// A new instance is created from the mapped class.
    /// </summary>
    /// <param name="classname">Name of the mapped class</param>
    /// <param name="requiredType">Required type</param>
    /// <returns></returns>
    private object CreateInstanceOfMappedClass(string classname, Type requiredType)
    {
      if (string.IsNullOrWhiteSpace(classname))
      {
        throw new TypeClassMapperException($"Mapped class for type [{requiredType.FullName}] cannot be empty: [{classname}] at configured scope [{scope ?? "<default>"}] and section [{section ?? "<default>"}]");
      }
      Type mappedClass = GetClass(classname);
      if (mappedClass == null)
      {
        throw new TypeClassMapperException($"Mapped class for type [{requiredType.FullName}] not found: [{classname}] at configured scope [{scope ?? "<default>"}] and section [{section ?? "<default>"}]");
      }
      try
      {
        return System.Activator.CreateInstance(mappedClass);
      }
      catch (System.Reflection.TargetInvocationException exception)
      {
        throw new TypeClassMapperException($"Cannot create an instance of [{mappedClass}] at configured scope [{scope ?? "<default>"}] and section [{section ?? "<default>"}]. Check InnerException.", exception.InnerException);
      }
      catch (Exception exception)
      {
        throw new TypeClassMapperException($"Cannot create an instance of [{mappedClass}] at configured scope [{scope ?? "<default>"}] and section [{section ?? "<default>"}]. Check InnerException.", exception);
      }
    }

    /// <summary>
    /// Uses the most basic .NET mechanism to Type/Class resolution (including in-process and also assembly resolution).
    /// </summary>
    /// <param name="classname">Fully qualified Type name</param>
    /// <returns>Required Type</returns>
    private Type GetClass(string classname)
    {
      return System.Type.GetType(classname);
    }

    private TypeClassMapperConfigurationSection GetConfiguration(string section = null)
    {
      const string TypeClassMapperConfigurationSectionName = nameof(TypeClassMapperConfigurationSection);
      string section_name = TypeClassMapperConfigurationSectionName;
      if (!string.IsNullOrWhiteSpace(section))
      {
        section_name = section;
      }
      System.Configuration.ConfigurationManager.RefreshSection(section_name);
      TypeClassMapperConfigurationSection result = System.Configuration.ConfigurationManager.GetSection(section_name) as TypeClassMapperConfigurationSection;
      if (result == null)
      {
        throw new System.Configuration.ConfigurationErrorsException($"{TypeClassMapperConfigurationSectionName} is not properly configured.");
      }
      return result;
    }

    private void InitAndLoadMappings(TypeClassMapperConfigurationSection configSection, string scope = null)
    {
      typemap = new Dictionary<string, object>();
      if (string.IsNullOrWhiteSpace(scope))
      {
        foreach (MappingCollectionElement mapping in configSection.Mappings)
        {
          typemap.Add(mapping.Type, mapping.Class);
        }
      }
      else
      {
        foreach (MappingCollectionElement mapping in configSection.Scopes.GetScopeMappings(scope))
        {
          typemap.Add(mapping.Type, mapping.Class);
        }
      }
    }
  }
}