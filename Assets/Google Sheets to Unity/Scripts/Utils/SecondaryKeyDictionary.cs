using System;
using System.Collections.Generic;
using UnityEngine;

namespace GreenerGames
{

    /// <summary>
    /// 2 key dictionary when key 1 and 2 are of different types
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="V"></typeparam>
    [Serializable]
    public class SecondaryKeyDictionary<T1, T2, V>
    {
        public Dictionary<T1, V> primaryDictionary = new Dictionary<T1, V>();
        public Dictionary<T2, T1> secondaryKeyLink = new Dictionary<T2, T1>();

        public V this[T1 primary]
        {
            get { return GetValueFromPrimary(primary); }
        }

        public V this[T2 secondary]
        {
            get { return GetValueFromSecondary(secondary); }
        }

        /// <summary>
        ///     This is used to attempt to grab from primary first, if not match will attempt to find from secondary keys
        ///     in the case of both type being the same, pass them both in through this to attempt to grab either
        /// </summary>
        /// <param name="primary"></param>
        /// <param name="secondary"></param>
        /// <exception cref="KeyNotFoundException"></exception>
        public V this[T1 primary, T2 secondary]
        {
            get { return GetValueFromEither(primary, secondary); }
        }

        /// <summary>
        ///     Gets the value based on the primary key
        /// </summary>
        /// <param name="primaryKey"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public V GetValueFromPrimary(T1 primaryKey)
        {
            if (primaryDictionary.ContainsKey(primaryKey)) return primaryDictionary[primaryKey];

            throw new KeyNotFoundException("primary key not found");
        }

        /// <summary>
        ///     Gets the value from the secondary key
        /// </summary>
        /// <param name="secondaryKey"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public V GetValueFromSecondary(T2 secondaryKey)
        {
            if (secondaryKeyLink.ContainsKey(secondaryKey))
            {
                var primarykey = secondaryKeyLink[secondaryKey];
                return GetValueFromPrimary(primarykey);
            }

            throw new KeyNotFoundException("Secondary not found");
        }

        /// <summary>
        ///     Try get value from either based searching with primary key first
        /// </summary>
        /// <param name="primary"></param>
        /// <param name="secondary"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public V GetValueFromEither(T1 primary, T2 secondary)
        {
            if (primaryDictionary.ContainsKey(primary)) return GetValueFromPrimary(primary);

            if (secondaryKeyLink.ContainsKey(secondary)) return GetValueFromSecondary(secondary);

            throw new KeyNotFoundException("Key not found");
        }

        /// <summary>
        /// Add an entry with only a primary key, can be linked at a later time
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void Add(T1 key, V value)
        {
            if (primaryDictionary.ContainsKey(key)) throw new InvalidOperationException("Primary key already exist");

            primaryDictionary.Add(key, value);
        }

        /// <summary>
        /// add an entry with a primary and secondary key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="secondaryKey"></param>
        public void Add(T1 key, V value, T2 secondaryKey)
        {
            Add(key, value);

            LinkSecondaryKey(key, secondaryKey);
        }

        /// <summary>
        /// Link a secondary key to a primary key
        /// </summary>
        /// <param name="primaryKey"></param>
        /// <param name="secondaryKey"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void LinkSecondaryKey(T1 primaryKey, T2 secondaryKey)
        {
            if (primaryDictionary.ContainsKey(primaryKey))
                if (secondaryKeyLink.ContainsKey(secondaryKey))
                {
                    //adding a key to an already existing secondary key, this will override the previous key
                    Debug.LogWarning("Secondary key already exists, replacing orginal key with new one");
                    secondaryKeyLink[secondaryKey] = primaryKey;
                }
                else
                {
                    secondaryKeyLink.Add(secondaryKey, primaryKey);
                }
            else
                throw new InvalidOperationException("Secondary key already exist");
        }

        public bool ContainsPrimaryKey(T1 primaryKey)
        {
            return primaryDictionary.ContainsKey(primaryKey);
        }

        public bool ContainsSecondaryKey(T2 secondaryKey)
        {
            if (secondaryKeyLink.ContainsKey(secondaryKey))
            {
                var primarykey = secondaryKeyLink[secondaryKey];

                return primaryDictionary.ContainsKey(primarykey);
            }

            return false;
        }

        public bool ContainsKey(T1 key)
        {
            return ContainsPrimaryKey(key);
        }

        public bool ContainsKey(T2 secondaryKey)
        {
            return ContainsSecondaryKey(secondaryKey);
        }
    }

    /// <summary>
    /// 2 Key dictionary when value 1 and 2 are of different types
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="V"></typeparam>
    [Serializable]
    public class SecondaryKeyDictionary<T, V>
    {
        public Dictionary<T, V> primaryDictionary = new Dictionary<T, V>();
        public Dictionary<T, T> secondaryKeyLink = new Dictionary<T, T>();

        /// <summary>
        ///     This is used to attempt to grab from primary first, if not match will attempt to find from secondary keys
        ///     in the case of both type being the same, pass them both in through this to attempt to grab either
        /// </summary>
        /// <param name="key"></param>
        /// <exception cref="KeyNotFoundException"></exception>
        public V this[T key]
        {
            get { return GetValueFromEither(key); }
        }

        /// <summary>
        ///     Gets the value based on the primary key
        /// </summary>
        /// <param name="primaryKey"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public V GetValueFromPrimary(T primaryKey)
        {
            if (primaryDictionary.ContainsKey(primaryKey)) return primaryDictionary[primaryKey];

            throw new KeyNotFoundException("primary key not found");
        }

        /// <summary>
        ///     Gets the value from the secondary key
        /// </summary>
        /// <param name="secondaryKey"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public V GetValueFromSecondary(T secondaryKey)
        {
            if (secondaryKeyLink.ContainsKey(secondaryKey))
            {
                var primarykey = secondaryKeyLink[secondaryKey];

                return GetValueFromPrimary(primarykey);
            }

            throw new KeyNotFoundException("Secondary not found");
        }

        /// <summary>
        ///     Try get value from either based searching with primary key first
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public V GetValueFromEither(T key)
        {
            if (primaryDictionary.ContainsKey(key)) return GetValueFromPrimary(key);

            if (secondaryKeyLink.ContainsKey(key)) return GetValueFromSecondary(key);

            throw new KeyNotFoundException("Key not found");
        }

        /// <summary>
        /// Add an entry with only a primary key, can be linked at a later time
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void Add(T key, V value)
        {
            if (primaryDictionary.ContainsKey(key)) throw new InvalidOperationException("Primary key already exist");

            primaryDictionary.Add(key, value);
        }

        /// <summary>
        /// add an entry with a primary and secondary key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="secondaryKey"></param>
        public void Add(T key, V value, T secondaryKey)
        {
            Add(key, value);

            LinkSecondaryKey(key, secondaryKey);
        }

        /// <summary>
        /// Link a secondary key to a primary key
        /// </summary>
        /// <param name="primaryKey"></param>
        /// <param name="secondaryKey"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void LinkSecondaryKey(T primaryKey, T secondaryKey)
        {
            if (primaryDictionary.ContainsKey(primaryKey))
                if (secondaryKeyLink.ContainsKey(secondaryKey))
                {
                    //adding a key to an already existing secondary key, this will override the previous key
                    Debug.LogWarning("Secondary key already exists, replacing orginal key with new one");
                    secondaryKeyLink[secondaryKey] = primaryKey;
                }
                else
                {
                    secondaryKeyLink.Add(secondaryKey, primaryKey);
                }
            else
                throw new InvalidOperationException("Secondary key already exist");
        }


        public bool ContainsKey(T key)
        {
            if (ContainsPrimaryKey(key)) return true;

            return ContainsSecondaryKey(key);
        }

        public bool ContainsPrimaryKey(T primaryKey)
        {
            return primaryDictionary.ContainsKey(primaryKey);
        }

        public bool ContainsSecondaryKey(T secondaryKey)
        {
            if (secondaryKeyLink.ContainsKey(secondaryKey))
            {
                var key = secondaryKeyLink[secondaryKey];

                return primaryDictionary.ContainsKey(key);
            }

            return false;
        }
    }
}