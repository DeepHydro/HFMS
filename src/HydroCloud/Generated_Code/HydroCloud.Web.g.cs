//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace HydroCloud
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.ServiceModel.DomainServices;
    using System.ServiceModel.DomainServices.Client;
    using System.ServiceModel.DomainServices.Client.ApplicationServices;
    
    
    /// <summary>
    /// RIA 应用程序的上下文。
    /// </summary>
    /// <remarks>
    /// 此上下文对库进行了扩展，使得应用程序服务和类型
    /// 可供代码和 xaml 使用。
    /// </remarks>
    public sealed partial class WebContext : WebContextBase
    {
        
        #region 可扩展性方法定义

        /// <summary>
        /// 一旦初始化完成便从构造函数中调用此方法，
        /// 还可将此方法用于以后的对象设置。
        ///</summary>
        partial void OnCreated();

        #endregion
        
        
        /// <summary>
        /// 初始化 WebContext 类的新实例。
        /// </summary>
        public WebContext()
        {
            this.OnCreated();
        }
        
        /// <summary>
        /// 获取向当前应用程序注册为生存期对象的上下文。
        /// </summary>
        /// 如果没有当前的应用程序，没有添加上下文或添加了多个上下文，
        /// 则会引发 <exception cref="InvalidOperationException">。
        /// </exception>
        /// <seealso cref="System.Windows.Application.ApplicationLifetimeObjects"/>
        public new static WebContext Current
        {
            get
            {
                return ((WebContext)(WebContextBase.Current));
            }
        }
    }
}