#pragma once
#include "pch.h"
#include <stdexcept>
#include <cassert>
#include <array>

namespace ImageFramework::detail
{
	class Pipeline
	{
	public:
		enum Type
		{
			StdIn,
			StdOut
		};

		Pipeline(Type type) : m_type(type)
		{
			SECURITY_ATTRIBUTES attribs;
			attribs.nLength = sizeof(attribs);
			attribs.bInheritHandle = TRUE;
			attribs.lpSecurityDescriptor = nullptr;

			if (!CreatePipe(&m_read, &m_write, &attribs, 0))
				throw std::runtime_error("CreatePipe");

			if(type == StdOut)
			{
				// ensure that read handle is not inherited
				if (!SetHandleInformation(m_read, HANDLE_FLAG_INHERIT, 0))
					throw std::runtime_error("SetHandleInformation");
			}
			else if(type == StdIn)
			{
				// ensure that write handle is not inherited
				if (!SetHandleInformation(m_write, HANDLE_FLAG_INHERIT, 0))
					throw std::runtime_error("SetHandleInformation");
			}
		}

		HANDLE GetRead() const
		{
			assert(m_type == StdIn);
			return m_read;
		}

		HANDLE GetWrite() const
		{
			assert(m_type == StdOut);
			return m_write;
		}

		void Write(std::string_view text)
		{
			assert(m_type == StdIn);
			DWORD written = 0;
			if (!WriteFile(m_write, text.data(), DWORD(text.size()), &written, NULL))
				throw std::runtime_error("WriteFile");
		}

		bool CanRead() const
		{
			assert(m_type == StdOut);

			if (!m_pendingRead.empty()) return true;

			DWORD available = 0;
			if (!PeekNamedPipe(m_read, nullptr, 0, nullptr, &available, nullptr))
				throw std::runtime_error("PeekNamedPipe");

			return available > 0;
		}

		std::string ReadLine() const
		{
			assert(m_type == StdOut);

			std::string result;
			std::array<char, 4096> buffer;

			do
			{
				if (!m_pendingRead.empty())
				{
					// append until end of line
					auto it = m_pendingRead.find_first_of('\n');
					if (it != std::string::npos)
					{
						// only append until end of line and return
						result.append(m_pendingRead.c_str(), it);
						m_pendingRead = m_pendingRead.substr(it + 1);

						if (result.back() == '\r')
							result.pop_back();

						return result;
					}

					// append all and wait for more
					result.append(m_pendingRead);
					m_pendingRead.resize(0);
				}

				DWORD numRead = DWORD(buffer.size());
				
				if (!ReadFile(m_read, buffer.data(), DWORD(buffer.size()), &numRead, NULL))
					throw std::runtime_error("ReadFile");

				m_pendingRead.append(buffer.data(), numRead);
			} while (true);
		}

		~Pipeline()
		{
			CloseHandle(m_read);
			CloseHandle(m_write);
		}

	private:
		mutable std::string m_pendingRead;

		HANDLE m_read = nullptr;
		HANDLE m_write = nullptr;
		Type m_type;
	};
}
