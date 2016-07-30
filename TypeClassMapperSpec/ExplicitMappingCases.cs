﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

#region Types & Classes used by the test cases of the following TestClasses: ExplicitMappingCases, ImplicitMappingCases, ImplicitMappingCasesAtNoDefaultSection, InterfaceAugmentationCases

namespace app1
{
  public interface ISource
  {
    string Name { get; }
  }
}

namespace module1
{
  public class Source : app1.ISource
  {
    public string Name { get { return GetType().FullName; } }
  }
}

namespace module2
{
  public class Source : app1.ISource
  {
    static Source()
    {
      throw new Exception(".NET type initialization problem");
    }
    public string Name { get { return GetType().FullName; } }
  }
}

namespace module3
{
  public abstract class Source : app1.ISource
  {
    public abstract string Name { get; }
  }
  public class Source1 : app1.ISource
  {
    private string typemap_ctor;
    public Source1(System.IServiceProvider typemap)
    {
      this.typemap_ctor = "System.IServiceProvider";
    }
    public Source1(nutility.ITypeClassMapper typemap)
    {
      this.typemap_ctor = "nutility.ITypeClassMapper";
    }
    public Source1(nutility.TypeClassMapper typemap)
    {
      this.typemap_ctor = "nutility.TypeClassMapper";
    }
    public string Name => typemap_ctor;
  }
}

#endregion

namespace TypeClassMapperSpec
{
  [TestClass]
  public class ExplicitMappingCases
  {
    [TestMethod]
    public void VeryBasicUseCase()
    {
      //Arrange
      var typemap = new nutility.TypeClassMapper(new Dictionary<nutility.TypeClassID, nutility.TypeClassID> { { "app1.ISource", "module1.Source, TypeClassMapperSpec" } });

      //Act
      var instance = (app1.ISource)typemap.GetService(typeof(app1.ISource));

      //Assert
      Assert.AreEqual(typeof(module1.Source).FullName, instance.Name);
    }

    [TestMethod]
    public void BadMappings()
    {
      //Arrange
      IDictionary<nutility.TypeClassID, nutility.TypeClassID> mappings = null;

      //Act
      Exception exception = null;
      try
      {
        var typemap = new nutility.TypeClassMapper(typeclassmap: mappings);
      }
      catch (Exception ex)
      {
        exception = ex;
      }

      //Assert
      Assert.IsNotNull(exception);
      Assert.AreEqual<Type>(typeof(nutility.TypeClassMapperException), exception.GetType());
      Assert.AreEqual<string>("Parameter cannot be null: typeclassmap", exception.Message);
      Assert.IsInstanceOfType(exception.InnerException, typeof(ArgumentNullException));
    }

    [TestMethod]
    public void BadRequiredType()
    {
      //Arrange
      var typemap = new nutility.TypeClassMapper(new Dictionary<nutility.TypeClassID, object> { { "app1.ISource", "bad.Source, TypeClassMapperSpec" } });

      //Act
      Exception exception = null;
      try
      {
        var instance = (app1.ISource)typemap.GetService(requiredType: null);
      }
      catch (Exception ex)
      {
        exception = ex;
      }

      //Assert
      Assert.IsNotNull(exception);
      Assert.AreEqual<Type>(typeof(nutility.TypeClassMapperException), exception.GetType());
      Assert.AreEqual<string>("Parameter cannot be null: requiredType", exception.Message);
      Assert.IsInstanceOfType(exception.InnerException, typeof(ArgumentNullException));
    }

    [TestMethod]
    public void BadTypeName()
    {
      //Arrange
      var typemap = new nutility.TypeClassMapper(new Dictionary<nutility.TypeClassID, nutility.TypeClassID> { { "bad.ISource", "module1.Source, TypeClassMapperSpec" } });

      //Act
      Exception exception = null;
      try
      {
        var instance = (app1.ISource)typemap.GetService(typeof(app1.ISource));
      }
      catch (Exception ex)
      {
        exception = ex;
      }

      //Assert
      Assert.IsNotNull(exception);
      Assert.AreEqual<Type>(typeof(nutility.TypeClassMapperException), exception.GetType());
      Assert.AreEqual<string>("Type not found: [app1.ISource] at configured scope [<explicit>] and section [<explicit>]", exception.Message);
      Assert.IsNull(exception.InnerException);
    }

    [TestMethod]
    public void BadClassName()
    {
      //Arrange
      var typemap = new nutility.TypeClassMapper(new Dictionary<nutility.TypeClassID, nutility.TypeClassID> { { "app1.ISource", "bad.Source, TypeClassMapperSpec" } });

      //Act
      Exception exception = null;
      try
      {
        var instance = (app1.ISource)typemap.GetService(typeof(app1.ISource));
      }
      catch (Exception ex)
      {
        exception = ex;
      }

      //Assert
      Assert.IsNotNull(exception);
      Assert.AreEqual<Type>(typeof(nutility.TypeClassMapperException), exception.GetType());
      Assert.AreEqual<string>("Mapped class for type [app1.ISource] not found: [bad.Source, TypeClassMapperSpec] at configured scope [<explicit>] and section [<explicit>]", exception.Message);
      Assert.IsNull(exception.InnerException);
    }

    [TestMethod]
    public void BadClassName2()
    {
      //Arrange
      var typemap = new nutility.TypeClassMapper(new Dictionary<Type, nutility.TypeClassID> { { typeof(app1.ISource), "bad.Source, TypeClassMapperSpec" } });

      //Act
      Exception exception = null;
      try
      {
        var instance = (app1.ISource)typemap.GetService(typeof(app1.ISource));
      }
      catch (Exception ex)
      {
        exception = ex;
      }

      //Assert
      Assert.IsNotNull(exception);
      Assert.AreEqual<Type>(typeof(nutility.TypeClassMapperException), exception.GetType());
      Assert.AreEqual<string>("Mapped class for type [app1.ISource] not found: [bad.Source, TypeClassMapperSpec] at configured scope [<explicit>] and section [<explicit>]", exception.Message);
      Assert.IsNull(exception.InnerException);
    }

    [TestMethod]
    public void BadEmptyClassName()
    {
      //Arrange
      var typemap = new nutility.TypeClassMapper(new Dictionary<nutility.TypeClassID, nutility.TypeClassID> { { "app1.ISource", "" } });

      //Act
      Exception exception = null;
      try
      {
        var instance = (app1.ISource)typemap.GetService(typeof(app1.ISource));
      }
      catch (Exception ex)
      {
        exception = ex;
      }

      //Assert
      Assert.IsNotNull(exception);
      Assert.AreEqual<Type>(typeof(nutility.TypeClassMapperException), exception.GetType());
      Assert.AreEqual<string>("Mapped class for type [app1.ISource] cannot be empty: [] at configured scope [<explicit>] and section [<explicit>]", exception.Message);
      Assert.IsNull(exception.InnerException);
    }

    [TestMethod]
    public void BadEmptyClassName2()
    {
      //Arrange
      var typemap = new nutility.TypeClassMapper(new Dictionary<Type, nutility.TypeClassID> { { typeof(app1.ISource), "" } });

      //Act
      Exception exception = null;
      try
      {
        var instance = (app1.ISource)typemap.GetService(typeof(app1.ISource));
      }
      catch (Exception ex)
      {
        exception = ex;
      }

      //Assert
      Assert.IsNotNull(exception);
      Assert.AreEqual<Type>(typeof(nutility.TypeClassMapperException), exception.GetType());
      Assert.AreEqual<string>("Mapped class for type [app1.ISource] cannot be empty: [] at configured scope [<explicit>] and section [<explicit>]", exception.Message);
      Assert.IsNull(exception.InnerException);
    }

    [TestMethod]
    public void BadNullClassName()
    {
      //Arrange
      var typemap = new nutility.TypeClassMapper(new Dictionary<Type, nutility.TypeClassID> { { typeof(app1.ISource), null } });

      //Act
      Exception exception = null;
      try
      {
        var instance = (app1.ISource)typemap.GetService(typeof(app1.ISource));
      }
      catch (Exception ex)
      {
        exception = ex;
      }

      //Assert
      Assert.IsNotNull(exception);
      Assert.AreEqual<Type>(typeof(nutility.TypeClassMapperException), exception.GetType());
      Assert.AreEqual<string>("Mapped class for type [app1.ISource] cannot be null: at configured scope [<explicit>] and section [<explicit>]", exception.Message);
      Assert.IsNull(exception.InnerException);
    }

    [TestMethod]
    public void NullMappedInstance()
    {
      //Arrange
      var typemap = new nutility.TypeClassMapper(new Dictionary<nutility.TypeClassID, object> { { "app1.ISource", null } });

      //Act
      var instance = (app1.ISource)typemap.GetService(typeof(app1.ISource));

      //Assert
      Assert.IsNull(instance);
    }

    [TestMethod]
    public void NullMappedInstance2()
    {
      //Arrange
      var typemap = new nutility.TypeClassMapper(new Dictionary<Type, object> { { typeof(app1.ISource), null } });

      //Act
      var instance = (app1.ISource)typemap.GetService(typeof(app1.ISource));

      //Assert
      Assert.IsNull(instance);
    }

    [TestMethod]
    public void BadTypeInit()
    {
      //Arrange
      var typemap = new nutility.TypeClassMapper(new Dictionary<nutility.TypeClassID, nutility.TypeClassID> { { "app1.ISource", "module2.Source, TypeClassMapperSpec" } });

      //Act
      Exception exception = null;
      try
      {
        var instance = (app1.ISource)typemap.GetService(typeof(app1.ISource));
      }
      catch (Exception ex)
      {
        exception = ex;
      }

      //Assert
      Assert.IsNotNull(exception);
      Assert.AreEqual<Type>(typeof(nutility.TypeClassMapperException), exception.GetType());
      Assert.AreEqual<string>("Cannot create an instance of [module2.Source] at configured scope [<explicit>] and section [<explicit>]. Check InnerException.", exception.Message);
      Assert.IsNotNull(exception.InnerException);
      Assert.IsInstanceOfType(exception.InnerException, typeof(System.TypeInitializationException));
      Assert.AreEqual<string>("The type initializer for 'module2.Source' threw an exception.", exception.InnerException.Message);
    }

    [TestMethod]
    public void BadTypeInit2()
    {
      //Arrange
      var typemap = new nutility.TypeClassMapper(new Dictionary<Type, Type> { { typeof(app1.ISource), typeof(module2.Source) } });

      //Act
      Exception exception = null;
      try
      {
        var instance = (app1.ISource)typemap.GetService(typeof(app1.ISource));
      }
      catch (Exception ex)
      {
        exception = ex;
      }

      //Assert
      Assert.IsNotNull(exception);
      Assert.AreEqual<Type>(typeof(nutility.TypeClassMapperException), exception.GetType());
      Assert.AreEqual<string>("Cannot create an instance of [module2.Source] at configured scope [<explicit>] and section [<explicit>]. Check InnerException.", exception.Message);
      Assert.IsNotNull(exception.InnerException);
      Assert.IsInstanceOfType(exception.InnerException, typeof(System.TypeInitializationException));
      Assert.AreEqual<string>("The type initializer for 'module2.Source' threw an exception.", exception.InnerException.Message);
    }

    [TestMethod]
    public void BadTypeInit3()
    {
      //Arrange
      var typemap = new nutility.TypeClassMapper(new Dictionary<Type, nutility.TypeClassID> { { typeof(app1.ISource), "module2.Source, TypeClassMapperSpec" } });

      //Act
      Exception exception = null;
      try
      {
        var instance = (app1.ISource)typemap.GetService(typeof(app1.ISource));
      }
      catch (Exception ex)
      {
        exception = ex;
      }

      //Assert
      Assert.IsNotNull(exception);
      Assert.AreEqual<Type>(typeof(nutility.TypeClassMapperException), exception.GetType());
      Assert.AreEqual<string>("Cannot create an instance of [module2.Source] at configured scope [<explicit>] and section [<explicit>]. Check InnerException.", exception.Message);
      Assert.IsNotNull(exception.InnerException);
      Assert.IsInstanceOfType(exception.InnerException, typeof(System.TypeInitializationException));
      Assert.AreEqual<string>("The type initializer for 'module2.Source' threw an exception.", exception.InnerException.Message);
    }

    [TestMethod]
    public void BadTypeLoad()
    {
      //Arrange
      var typemap = new nutility.TypeClassMapper(new Dictionary<nutility.TypeClassID, nutility.TypeClassID> { { "app1.ISource", "module3.Source, TypeClassMapperSpec" } });

      //Act
      Exception exception = null;
      try
      {
        var instance = (app1.ISource)typemap.GetService(typeof(app1.ISource));
      }
      catch (Exception ex)
      {
        exception = ex;
      }

      //Assert
      Assert.IsNotNull(exception);
      Assert.AreEqual<Type>(typeof(nutility.TypeClassMapperException), exception.GetType());
      Assert.AreEqual<string>("Cannot create an instance of [module3.Source] at configured scope [<explicit>] and section [<explicit>]. Check InnerException.", exception.Message);
      Assert.IsNotNull(exception.InnerException);
      Assert.IsInstanceOfType(exception.InnerException, typeof(System.MissingMethodException));
      Assert.AreEqual<string>("Cannot create an abstract class.", exception.InnerException.Message);
    }

    [TestMethod]
    public void BadTypeLoad2()
    {
      //Arrange
      var typemap = new nutility.TypeClassMapper(new Dictionary<Type, Type> { { typeof(app1.ISource), typeof(module3.Source) } });

      //Act
      Exception exception = null;
      try
      {
        var instance = (app1.ISource)typemap.GetService(typeof(app1.ISource));
      }
      catch (Exception ex)
      {
        exception = ex;
      }

      //Assert
      Assert.IsNotNull(exception);
      Assert.AreEqual<Type>(typeof(nutility.TypeClassMapperException), exception.GetType());
      Assert.AreEqual<string>("Cannot create an instance of [module3.Source] at configured scope [<explicit>] and section [<explicit>]. Check InnerException.", exception.Message);
      Assert.IsNotNull(exception.InnerException);
      Assert.IsInstanceOfType(exception.InnerException, typeof(System.MissingMethodException));
      Assert.AreEqual<string>("Cannot create an abstract class.", exception.InnerException.Message);
    }

    [TestMethod]
    public void BadTypeLoad3()
    {
      //Arrange
      var typemap = new nutility.TypeClassMapper(new Dictionary<Type, nutility.TypeClassID> { { typeof(app1.ISource), "module3.Source, TypeClassMapperSpec" } });

      //Act
      Exception exception = null;
      try
      {
        var instance = (app1.ISource)typemap.GetService(typeof(app1.ISource));
      }
      catch (Exception ex)
      {
        exception = ex;
      }

      //Assert
      Assert.IsNotNull(exception);
      Assert.AreEqual<Type>(typeof(nutility.TypeClassMapperException), exception.GetType());
      Assert.AreEqual<string>("Cannot create an instance of [module3.Source] at configured scope [<explicit>] and section [<explicit>]. Check InnerException.", exception.Message);
      Assert.IsNotNull(exception.InnerException);
      Assert.IsInstanceOfType(exception.InnerException, typeof(System.MissingMethodException));
      Assert.AreEqual<string>("Cannot create an abstract class.", exception.InnerException.Message);
    }

    [TestMethod]
    public void theTypeForMyCase()
    {
      //Arrange
      var typemap = new nutility.TypeClassMapper
      (
        new List<nutility.MappedTypes>
        {
          new nutility.MappedTypes { RequiredType = typeof(app1.ISource), ClientType = typeof(ExplicitMappingCases), MappedClass = typeof(module3.Source1) }
        }
      );

      //Act
      //app1.ISource source1 = typemap.GetService<app1.ISource>(System.Reflection.MethodBase.GetCurrentMethod().Name);
      app1.ISource source2 = typemap.GetService<app1.ISource>(client_type: typeof(ExplicitMappingCases));

      //Assert
//      Assert.AreEqual<string>("1", source1.Name);
      Assert.AreEqual<string>("nutility.TypeClassMapper", source2.Name);
    }
  }
}