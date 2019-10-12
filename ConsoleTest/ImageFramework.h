#pragma once
#include "pch.h"
#include <stdexcept>
#include <minwinbase.h>
#include "Pipeline.h"

namespace ImageFramework
{
	class Model
	{
	public:
		Model() : 
		m_info({}),
		m_in(detail::Pipeline::StdIn), m_out(detail::Pipeline::StdOut), m_err(detail::Pipeline::StdOut)
		{
			STARTUPINFOA st;
			ZeroMemory(&st, sizeof(st));
			st.cb = sizeof(st);
			st.hStdError = m_err.GetWrite();
			st.hStdOutput = m_out.GetWrite();
			st.hStdInput = m_in.GetRead();
			st.dwFlags = STARTF_USESTDHANDLES;
			//st.dwX = 10;
			//st.dwY = 10;
			//st.dwXSize = 200;
			//st.dwYSize = 200;
			//st.dwXCountChars = 50;
			//st.dwYCountChars = 20;
			//st.dwFlags |= STARTF_USEPOSITION | STARTF_USESIZE | STARTF_USECOUNTCHARS | STARTF_USESHOWWINDOW;
			//st.wShowWindow = SW_SHOW;

			char args[] = "ImageConsole.exe -cin";
			if (!CreateProcessA(
				"ImageConsole.exe",
				args,
				nullptr,
				nullptr,
				TRUE,
				NORMAL_PRIORITY_CLASS | CREATE_NO_WINDOW,
				nullptr,
				nullptr,
				&st,
				&m_info
			)) throw std::runtime_error("could not launch ImageConsole.exe");
		}

		~Model()
		{
			try
			{
				m_in.Write("-close\n");
			}catch (...){}

			WaitForSingleObject(m_info.hProcess, INFINITE);

			CloseHandle(m_info.hProcess);
			CloseHandle(m_info.hThread);
		}

		void Open(std::string_view filename)
		{
			m_in.Write("-open \"");
			m_in.Write(std::string(filename));
			m_in.Write("\"\n");

			auto err = m_err.Read(true);
			if (!err.empty())
				throw std::runtime_error(err);
		}

		int GetNumLayers()
		{
			m_in.Write("-telllayers\n");

			auto res = m_out.Read(false);
			assert(!res.empty());
			return std::stol(res);
		}
	private:

	private:
		PROCESS_INFORMATION m_info;
		detail::Pipeline m_in;
		detail::Pipeline m_out;
		detail::Pipeline m_err;
	};
}

