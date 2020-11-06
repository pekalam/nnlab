﻿namespace Common.Framework
{
    /// <summary>
    /// Marker interface
    /// </summary>
    public interface ITransientController
    {
        void Initialize(IViewModel vm);
    }


    public class ControllerBase<TVm> : ITransientController where TVm : ViewModelBase<TVm>
    {
        protected TVm? Vm;

        public void Initialize(IViewModel vm)
        {
            Vm = vm as TVm;
            VmCreated();
        }

        protected virtual void VmCreated() { }

    }
}