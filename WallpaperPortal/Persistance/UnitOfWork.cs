﻿using WallpaperPortal.Models;
using WallpaperPortal.Repositories;

namespace WallpaperPortal.Persistance
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationContext _context;
        private RepositoryBase<User> _userRepository;
        private RepositoryBase<File> _fileRepository;
        private RepositoryBase<Tag> _tageRepository;


        public UnitOfWork(ApplicationContext context)
        {
            _context = context;
        }

        public ApplicationContext Context
        { 
            get 
            { 
                return _context; 
            } 
        }

        public RepositoryBase<User> UserRepository
        {
            get
            {
                if (_userRepository == null)
                {
                    _userRepository = new RepositoryBase<User>(_context);
                }
                return _userRepository;
            }
        }

        public RepositoryBase<File> FileRepository
        {
            get
            {
                if (_fileRepository == null)
                {
                    _fileRepository = new RepositoryBase<File>(_context);
                }
                return _fileRepository;
            }
        }

        public RepositoryBase<Tag> TagRepository
        {
            get
            {
                if (_tageRepository == null)
                {
                    _tageRepository = new RepositoryBase<Tag>(_context);
                }
                return _tageRepository;
            }
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
