using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VITAP.Library
{
    [Serializable]
    public class TIFPageCollection : System.Collections.CollectionBase
    {
        private bool m_Disposed = false;

        #region CONSTURCTORS

        /// <summary>
        /// Default Constructor
        /// </summary>
        public TIFPageCollection() { }

        /// <summary>
        /// Disposes of the object by clearing the collection
        /// </summary>
        public void Dispose()
        {   //Make sure each image is disposed of properly
            foreach (System.Drawing.Image Img in this)
            {
                Img.Dispose();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            this.m_Disposed = true;
            this.Clear();
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Adds an item to the collection
        /// </summary>
        /// <param name="Obj"></param>
        public void Add(System.Drawing.Image Obj)
        {
            this.List.Add(Obj);
        }

        /// <summary>
        /// Indicates if the object exists in the collection
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        public bool Contains(System.Drawing.Image Obj)
        {
            return this.List.Contains(Obj);
        }

        /// <summary>
        /// Returns the indexOf the object
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        public int IndexOf(System.Drawing.Image Obj)
        {
            return this.List.IndexOf(Obj);
        }

        /// <summary>
        /// Inserts an object into the collection at the specifed index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="Obj"></param>
        public void Insert(int index, System.Drawing.Image Obj)
        {
            this.List.Insert(index, Obj);
        }

        /// <summary>
        /// Removes the object from the collection
        /// </summary>
        /// <param name="Obj"></param>
        public void Remove(System.Drawing.Image Obj)
        {
            this.List.Remove(Obj);
        }

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Returns a reference to the object at specified index
        /// </summary>
        /// <param name="index">Index of object</param>
        /// <returns></returns>
        public System.Drawing.Image this[int index]
        {
            get
            {
                return (System.Drawing.Image)this.List[index];
            }
            set
            {
                this.List[index] = value;
            }
        }

        #endregion

    }
}
