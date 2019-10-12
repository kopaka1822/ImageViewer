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

		void Write(const std::string& text)
		{
			assert(m_type == StdIn);
			DWORD written = 0;
			if (!WriteFile(m_write, text.data(), DWORD(text.size()), &written, NULL))
				throw std::runtime_error("WriteFile");
		}

		std::string Read(bool peek) const
		{
			assert(m_type == StdOut);

			std::array<char, 4096> buffer;
			DWORD numRead = DWORD(buffer.size());
			std::string result;

			if(peek)
			{
				DWORD available = 0;
				if (!PeekNamedPipe(m_read, NULL, 0, NULL, &available, NULL))
					throw std::runtime_error("PeekNamedPipe");

				if (!available) return result; // nothing to read yet
			}

			while (numRead >= buffer.size())
			{
				auto res = ReadFile(m_read, buffer.data(), DWORD(buffer.size()), &numRead, NULL);
				if (!res) break;
				result.append(buffer.data(), numRead);
			}

			return result;
		}

		~Pipeline()
		{
			CloseHandle(m_read);
			CloseHandle(m_write);
		}

	private:
		HANDLE m_read = nullptr;
		HANDLE m_write = nullptr;
		Type m_type;
	};
}
