#pragma once
#include "pch.h"
#include <stdexcept>
#include <minwinbase.h>
#include "Pipeline.h"
#include <vector>

namespace ImageFramework
{
	class Model
	{
	public:
		struct Int2
		{
			int x;
			int y;
		};

		/// \param consolePath location of ImageConsole.exe
		Model(const std::string& consolePath = "ImageConsole.exe");

		~Model();

		/// opens image
		void OpenImage(std::string_view filename);
		/// deletes image
		void DeleteImage(int index);
		/// deletes all images
		void ClearImages();
		/// moves image to a new index
		void MoveImage(int oldIndex, int newIndex);
		/// sets image combine equation
		void SetEquation(std::string_view color, std::string_view alpha = nullptr);

		/// add filter to the filter list
		void OpenFilter(std::string_view filename);
		/// deletes filter from filter list
		void DeleteFilter(int index);
		/// deletes all filter
		void ClearFilter();
		/// sets filter parameter of the filter at filterIndex
		void SetFilterParam(int filterIndex, std::string_view paramName, std::string_view value);

		int GetNumLayers() const;
		int GetNumMipmaps() const;
		/// generates mipmaps or overwrites existing ones
		void GenMipmaps();
		/// deletes all mipmaps
		void DeleteMipmaps();
		/// returns size of the mipmap
		Int2 GetSize(int mipmap) const;
		bool IsAlpha() const;

		void SetExportLayer(int layer);
		void SetExportMipmap(int mipmap);
		void SetExportQuality(int quality);
		void SetExportCropping(int xStart, int yStart, int xEnd, int yEnd);
		void DisableExportCropping();

		/// waits for all console input to be finished
		void Sync() const;

		/// generates a thumbnail with the specified size
		/// \param size max width/height
		/// \param dstWidth (out) width of the generated thumbnail
		/// \param dstHeight (out) height of the generated thumbnail
		std::vector<uint8_t> GenThumbnail(int size, int& dstWidth, int& dstHeight);

		// TODO export, stats, tellfilterparams, tellformats, tellpixel
	private:
		std::string ReadLine() const
		{
			while(true)
			{
				if (m_err.CanRead())
					throw std::runtime_error(m_err.ReadLine());

				if (m_out.CanRead())
					return m_out.ReadLine();

				Yield();
			}
		}

		std::vector<uint8_t> ReadBinary(DWORD numBytes)
		{
			return m_out.ReadBinary(numBytes);
		}

	private:
		PROCESS_INFORMATION m_info;
		mutable detail::Pipeline m_in;
		detail::Pipeline m_out;
		detail::Pipeline m_err;
	};

	inline Model::Model(const std::string& consolePath) :
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
		

		char args[] = "ImageConsole.exe -cin -silent";
		
		if (!CreateProcessA(
			consolePath.c_str(),
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

	inline Model::~Model()
	{
		try
		{
			m_in.Write("-close\n");
			m_in.Flush();
			TerminateProcess(m_info.hProcess, 0);
			WaitForSingleObject(m_info.hProcess, 10000);
		}
		catch (...)
		{}

		CloseHandle(m_info.hProcess);
		CloseHandle(m_info.hThread);
	}

	inline void Model::OpenImage(std::string_view filename)
	{
		m_in.Write("-open \"");
		m_in.Write(filename);
		m_in.Write("\"\n");
	}

	inline void Model::DeleteImage(int index)
	{
		m_in.Write("-delete ");
		m_in.Write(std::to_string(index));
		m_in.Write("\n");
	}

	inline void Model::ClearImages()
	{
		m_in.Write("-delete\n");
	}

	inline void Model::MoveImage(int oldIndex, int newIndex)
	{
		m_in.Write("-move ");
		m_in.Write(std::to_string(oldIndex));
		m_in.Write(" ");
		m_in.Write(std::to_string(newIndex));
		m_in.Write("\n");
	}

	inline void Model::OpenFilter(std::string_view filename)
	{
		m_in.Write("-addfilter \"");
		m_in.Write(filename);
		m_in.Write("\"\n");
	}

	inline void Model::DeleteFilter(int index)
	{
		m_in.Write("-deletefilter ");
		m_in.Write(std::to_string(index));
		m_in.Write("\n");
	}

	inline void Model::ClearFilter()
	{
		m_in.Write("-deletefilter\n");
	}

	inline void Model::SetFilterParam(int filterIndex, std::string_view paramName, std::string_view value)
	{
		m_in.Write("-filterparam ");
		m_in.Write(std::to_string(filterIndex));
		m_in.Write(" \"");
		m_in.Write(paramName);
		m_in.Write("\" ");
		m_in.Write(value);
		m_in.Write("\n");
	}

	inline void Model::SetEquation(std::string_view color, std::string_view alpha)
	{
		m_in.Write("-equation \"");
		m_in.Write(color);
		if (alpha.data())
		{
			m_in.Write("\" \"");
			m_in.Write(alpha);
		}
		m_in.Write("\"\n");
	}

	inline int Model::GetNumLayers() const
	{
		m_in.Write("-telllayers\n");
		return std::stol(ReadLine());
	}

	inline int Model::GetNumMipmaps() const
	{
		m_in.Write("-tellmipmaps\n");
		return std::stol(ReadLine());
	}

	inline void Model::GenMipmaps()
	{
		m_in.Write("-genmipmaps\n");
	}

	inline void Model::DeleteMipmaps()
	{
		m_in.Write("-deletemipmaps\n");
	}

	inline Model::Int2 Model::GetSize(int mipmap) const
	{
		m_in.Write("-tellsize");
		Int2 res;
		res.x = std::stol(ReadLine());
		res.y = std::stol(ReadLine());
		return res;
	}

	inline bool Model::IsAlpha() const
	{
		m_in.Write("-tellapha");
		return ReadLine() == "True";
	}

	inline void Model::SetExportLayer(int layer)
	{
		m_in.Write("-exportlayer ");
		m_in.Write(std::to_string(layer));
		m_in.Write("\n");
	}

	inline void Model::SetExportMipmap(int mipmap)
	{
		m_in.Write("-exportmipmap ");
		m_in.Write(std::to_string(mipmap));
		m_in.Write("\n");
	}

	inline void Model::SetExportQuality(int quality)
	{
		m_in.Write("-exportquality ");
		m_in.Write(std::to_string(quality));
		m_in.Write("\n");
	}

	inline void Model::SetExportCropping(int xStart, int yStart, int xEnd, int yEnd)
	{
		m_in.Write("-exportcrop true ");
		m_in.Write(std::to_string(xStart));
		m_in.Write(" ");
		m_in.Write(std::to_string(yStart));
		m_in.Write(" ");
		m_in.Write(std::to_string(xEnd));
		m_in.Write(" ");
		m_in.Write(std::to_string(yEnd));
		m_in.Write("\n");
	}

	inline void Model::DisableExportCropping()
	{
		m_in.Write("-exportcrop false\n");
	}

	inline void Model::Sync() const
	{
		// execute simple command where the client needs to respond
		m_in.Write("-telllayers\n");
		ReadLine();
	}

	inline std::vector<uint8_t> Model::GenThumbnail(int size, int& dstWidth, int& dstHeight)
	{
		m_in.Write("-thumbnail ");
		m_in.Write(std::to_string(size));
		m_in.Write("\n");

		dstWidth = std::stol(ReadLine());
		dstHeight = std::stol(ReadLine());
		return ReadBinary(dstWidth * dstHeight * 4);
	}
}

