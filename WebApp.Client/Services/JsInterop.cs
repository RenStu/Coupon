using Microsoft.AspNetCore.Blazor;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace WebApp.Client.Services
{
    public static class JsInterop
    {
        public static async Task Init()
        {
            await JSRuntime.Current.InvokeAsync<bool>("init");
        }

        public static async Task AfterRenderIndex()
        {
            await JSRuntime.Current.InvokeAsync<bool>("afterRenderIndex");
        }

        public static async Task AfterRenderOffers()
        {
            await JSRuntime.Current.InvokeAsync<bool>("afterRenderOffers");
        }
        
        public static async Task AfterRenderRegister()
        {
            await JSRuntime.Current.InvokeAsync<bool>("afterRenderRegister");
        }

        public static async Task AfterRenderCoupon()
        {
            await JSRuntime.Current.InvokeAsync<bool>("afterRenderCoupon");
        }

        public static async Task AfterRenderLogin()
        {
            await JSRuntime.Current.InvokeAsync<bool>("afterRenderLogin");
        }
    }
}
