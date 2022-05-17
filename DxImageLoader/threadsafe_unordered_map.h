#pragma once
#include <unordered_map>
#include <mutex>

// light wrapper around std::unordered_map to allow concurrent inserts and erase
template<class Key, class T>
class threadsafe_unordered_map
{
public:
	// inserts or replaced the element corresponding to key
	void insert(Key key, std::shared_ptr<T> value)
	{
		std::lock_guard<std::mutex> g(m_mutex);
		m_map[key] = std::move(value);
	}

	// erases the key from the map (if it exists)
	void erase(Key key)
	{
		std::lock_guard<std::mutex> g(m_mutex);
		auto it = m_map.find(key);
		if (it != m_map.end())
			m_map.erase(it);
	}

	// attempts to find the key and returns nullptr if not found
	std::shared_ptr<T> find(Key key)
	{
		std::lock_guard<std::mutex> g(m_mutex);
		auto it = m_map.find(key);
		if (it == m_map.end()) return std::shared_ptr<T>();
		return it->second;
	}
private:
	std::unordered_map<Key, std::shared_ptr<T>> m_map;
	std::mutex m_mutex;
};