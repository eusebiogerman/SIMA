using Microsoft.Extensions.Configuration;
using SIMA.Helper.Interfaces;
using SIMA.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using WpfJEG.net6;

namespace SIMA.Presentation.Repository
{
    public partial class WindowServices<T, V,P> : WindowServicesBase<T, V, P> , IUtilServices<V, P>
    {

        public WindowServices(IConfiguration config, IPaging page, IContextservices<T, V, P> services) : base(config, page, services)
        {
            InsertStatus("Initializing Window Services.......", Brushes.Blue);
        }
        public WindowServices( ICacheService cache, IConfiguration config, IPaging page, IContextservices<T, V, P> services) : base(cache,config, page, services)
        {
            InsertStatus("Initializing Window Services.......", Brushes.Blue);
        }


        /// <summary>
        /// Returns the object Generic Value P with passing the values of the Active Filter controls 
        /// </summary>
        /// <returns>Generic Value P </returns>
        /// <exception cref="NotImplementedException"></exception>
        public P activeFilters() 
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Returns the object Generic Value P with passing the values of the Active Filter controls 
        /// </summary>
        /// <param name="actparam">(Func<P>) Predicate function Only Single Object P </param>
        /// <returns>Generic Value P </returns>
        public P activeFilters(Func<P> actparam)
        {
            return actparam.Invoke();
        }
        /// <summary>
        /// Returns the object Generic Value P with passing the values of the Active Filter controls 
        /// </summary>
        /// <param name="FilterFieldClass">(string) additional field</param>
        /// <returns></returns>
        public P activeFilters(string FilterFieldClass) 
       {
            Type type = typeof(P);
            P obj = (P)Activator.CreateInstance(type);

            var fields = new Dictionary<string, object>
            {
                { "offset", Page.Offset },
                { "limit", Page.Limit }
            };
            fields.Add(FilterFieldClass, TxtResults?.Text??"");

            foreach (var field in fields)
            {
                var prop = type.GetProperty(field.Key);

                if (prop != null && prop.CanWrite)
                {
                    object value = field.Value;

                    if (value != null && !prop.PropertyType.IsInstanceOfType(value))
                    {
                        value = Convert.ChangeType(value, prop.PropertyType);
                    }

                    prop.SetValue(obj, value);
                }
            }
            return obj;

        }
        /// <summary>
        /// Fill the ChildLovtextbox Control
        /// </summary>
        /// <param name="result">(IEnumerable<LovObject>) </param>
        public void FillChildCombobox(IEnumerable<LovObject> result)
        {
            try
            {
                ChildLovtextbox.ItemsSource = result;
                if (!EditMode && !FromMain)
                {
                    ChildLovtextbox.SelectedIndex = 0;
                }
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                EditMode = false;

            }
            catch (Exception)
            {
                EditMode = false;

            }
        }
        /// <summary>
        /// Fill the ChildLovtextbox Control Given the Value Item of ParentLovtextbox
        /// </summary>
        /// <typeparam name="X">IObject</typeparam>
        /// <typeparam name="W">IView</typeparam>
        /// <typeparam name="Z">IParam</typeparam>
        /// <param name="sender">Item LovObject Value</param>
        /// <param name="servicestype">Type of IContextservices instances</param>
        /// <param name="KEY">Type: (int) Field Id Class<W> to fill the LovText </param>
        /// <param name="VALUE">Type: (string) Field Value Class<W> to fill the LovText </param>
        /// <param name="paramproperty">Predicate fields to Cast LovObject with "(W)"  </param>
        /// <returns></returns>
        public async Task FillChildComboboxAsync<X,W,Z>(object sender,Type servicestype, string KEY,string VALUE,params string[] paramproperty)
        {
            try
            {
                //Type defintition
                var paramtype = typeof(Z);
                var viewtype  = typeof(W);


                // To fill the LovTexbox with the Parameter Z is it necesary to instanciting
                // example : New ParamClass { Idclass = sendobj?.Id, Idclasdummy = dummy  }
                LovObject? sendobj = ParentLovtextbox?.getSelectedItem(sender);
                int? dummy = (sendobj?.Id == 0 || sendobj == null ) ? -1 : null;
                var valproperty = new int?[]{sendobj?.Id, dummy};
                var param = Activator.CreateInstance(paramtype);
                for (int i = 0; i < paramproperty.Length; i++)
                {
                    if (i > 2) 
                       break;
                    else 
                       paramtype.GetProperty(paramproperty[i])!.SetValue(param, valproperty[i]);
                }
                
                // Instances the object of IEnumerable W
                object? instances = Activator.CreateInstance(servicestype, new object[] { Config });
                IContextservices<X, W, Z>? cmbservice = instances as IContextservices<X, W, Z>;
                IEnumerable<W> cat = await cmbservice.GetByFilter((Z)param);

                // Predicate expresion  to Cast V to LovObject  
                // example : predicate = (p) => new LovObject { Id = p.VClassId, Value = VClassValue }
                Func<W, LovObject> predicate = (p) =>
                new LovObject
                {
                    Id = (int?)viewtype?.GetProperty(KEY)?.GetValue(p),
                    Value = (string?)viewtype?.GetProperty(VALUE)?.GetValue(p),
                };

                // fill combo given the Param : Z and IEnumrable : V Result servise GetByFilter   
                IEnumerable<LovObject> result = cat.Select(predicate);
                FillChildCombobox(result);
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                EditMode = false;
                CatchExceptionAndMsg(ex: ex, showmsg: false);
            }
            catch (Exception se)
            {
                EditMode = false;
                CatchExceptionAndMsg(ex: se, showmsg: false);
            }
        }
        /// <summary>
        /// Fill the GridListView
        /// </summary>
        /// <param name="result">(IEnumerable<V>) result IView Abstract Type collection result</param>
        public void Fill(IEnumerable<V> result)
        {
           Page.resetPage();
           GridListView.ItemsSource = result;
           UpdatePaging();
        }
        /// <summary>
        /// Fill the stock given the argument P
        /// </summary>
        /// <param name="param">(P) Generic type class value </param>
        /// <returns>async Task</returns>
        public async Task FillAsync(P param)
        {
            var result = await Service.GetByFilter(param);
            Fill(result);
        }
        /// <summary>
        /// Insert or Update DB services value 
        /// </summary>
        /// <param name="param">(T) Generic type class value  </param>
        /// <returns></returns>
        public async Task<bool> SetAsync(T param)
        {
            return await Service.Set(param);
        }
        /// <summary>
        /// Delete DB services value 
        /// </summary>
        /// <param name="param">Id row </param>
        /// <returns></returns>
        public async Task<int> DeleteAsync(int? id) {
            return await Service.Delete(id);
        }
        /// <summary>
        /// Open the Edit Form for the Stock select in the Gridview
        /// </summary>
        /// <param name="rizenumber">(string) Only Numbers(1...n) or Percent Value(1%...n%)  Percent value to ajust the Form Panel to Window</param>
        /// <param name="editraction">(Action) for action edit values setted </param>
        /// <returns></returns>
        public async Task Edit(string rizenumber = "40%", Action? editraction = null)
        {

            //Edit panel visualization
            FormIsOpen(true);
            ResizeGrid(rizenumber);

            if (EditMode || FromMain)
            {
                //await Task.Delay(500);
                editraction?.Invoke();
            }
            else
            {
                if (ChildLovtextbox != null)
                {
                    ChildLovtextbox.Clear();
                    ChildLovtextbox.SelectedIndex = 0;
                }

                if (ParentLovtextbox != null)
                    ParentLovtextbox.SelectedIndex = 0;

            }

            EditMode = false;
            FromMain = false;
        }

    }
}
