using System;
using ClockCardAplication.Models;

namespace ClockCardAplication.DAL
{
    public class UnitOfWork : IDisposable
    {
        private ClockContext context;
        private IGenericRepository<User> userRepository;
        private IGenericRepository<Customer> customerRepository;
        private IGenericRepository<Project> projectRepository;
        private IGenericRepository<TimeLog> timeLogRepository;

        public UnitOfWork()
        {
            this.context = new ClockContext();
        }

        public UnitOfWork(ClockContext fcontext)
        {
            this.context = fcontext;
        }

        public IGenericRepository<User> UserRepository
        {
            get
            {

                if (this.userRepository == null)
                {
                    this.userRepository = new GenericRepository<User>(context);
                }
                return userRepository;
            }
        }

        public IGenericRepository<Customer> CustomerRepository
        {
            get
            {

                if (this.customerRepository == null)
                {
                    this.customerRepository = new GenericRepository<Customer>(context);
                }
                return customerRepository;
            }
        }

        public IGenericRepository<Project> ProjectRepository
        {
            get
            {

                if (this.projectRepository == null)
                {
                    this.projectRepository = new GenericRepository<Project>(context);
                }
                return projectRepository;
            }
        }

        public IGenericRepository<TimeLog> TimeLogRepository
        {
            get
            {

                if (this.timeLogRepository == null)
                {
                    this.timeLogRepository = new GenericRepository<TimeLog>(context);
                }
                return timeLogRepository;
            }
        }

        public void Save()
        {
            context.SaveChanges();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}