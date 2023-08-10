using System.Threading.Tasks;

namespace Tests;

public interface IAdapterTests
{
    public void Test_Instantiation();
    public Task Test_Connect();
    public Task Test_Get_File_Async();
    public Task Test_Get_Directory_Async();
    public Task Test_Get_Files_Async();
    public Task Test_Get_Directories_Async();
    public Task Test_File_Exists_Async();
    public Task Test_Directory_Exists_Async();
    public Task Test_Create_Directory_Async();
    public Task Test_Delete_File_Async();
    public Task Test_Delete_Directory_Async();
    public Task Test_Read_File_Async();
    public Task Test_Read_Text_File_Async();
    public Task Test_Write_File_Async();
    public Task Test_Append_File_Async();
}