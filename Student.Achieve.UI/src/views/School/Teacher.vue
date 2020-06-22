<template>
  <section>
    <!--工具条-->
    <el-col :span="24" class="toolbar" style="padding-bottom: 0px;">
      <el-form :inline="true" :model="filters" @submit.native.prevent>
        <el-form-item>
          <el-input v-model="filters.name" placeholder="输入查询内容"></el-input>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="getTeacher">查询</el-button>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="addTeacher">新增</el-button>
        </el-form-item>
      </el-form>
    </el-col>

    <!--列表-->
    <el-table
      :data="Teacher"
      highlight-current-row
      v-loading="listLoading"
      @selection-change="selsChange"
      style="width: 100%;"
    >
      <el-table-column type="selection" width="50"></el-table-column>
      <el-table-column type="index" width="80"></el-table-column>
      <el-table-column prop="TeacherNo" label="教师编号" width sortable></el-table-column>
      <el-table-column prop="Name" label="教师" width sortable></el-table-column>
      <el-table-column prop="Account" label="登录账号" width sortable></el-table-column>
      <el-table-column label="授课情况" width="240" sortable>
        <template scope="scope">
          <span v-for="item in scope.row.cct">
            {{item.grade.Name+item.clazz.Name+"班"+item.course.Name}}
            &nbsp;&nbsp;|&nbsp;
          </span>
        </template>
      </el-table-column>

      <el-table-column label="操作" width="150">
        <template scope="scope">
          <el-button size="small" @click="editTeacher(scope.$index, scope.row)">编辑</el-button>
          <el-button type="danger" size="small" @click="deleteTeacher(scope.$index, scope.row)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <!--工具条-->
    <el-col :span="24" class="toolbar">
      <el-button type="danger" @click="batchRemove" :disabled="this.sels.length===0">批量删除</el-button>
      <el-pagination
        layout="prev, pager, next"
        @current-change="nextPageTeacher"
        :page-size="50"
        :total="total"
        style="float:right;"
      ></el-pagination>
    </el-col>


    <!--新增界面-->
    <el-dialog
      title="新增教师"
      :visible.sync="addFormVisible"
      v-model="addFormVisible"
      :close-on-click-modal="false"
    >
      <el-form :model="addForm" label-width="80px" :rules="addFormRules" ref="addForm">
        <el-form-item label="教师编号" prop="TeacherNo">
          <el-input v-model="addForm.TeacherNo" auto-complete="off"></el-input>
        </el-form-item>
        <el-form-item label="教师" prop="Name">
          <el-input v-model="addForm.Name" auto-complete="off"></el-input>
        </el-form-item>
        <el-form-item label="账号" prop="Account">
          <el-input v-model="addForm.Account" auto-complete="off"></el-input>
        </el-form-item>

        <el-form-item label="年级" prop="gradeId">
          <el-select @change="getClazzTreeFunc"   v-model="addForm.gradeId" placeholder="请选择">
            <el-option
              v-for="item in GradeTree"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            ></el-option>
          </el-select>
        </el-form-item>

        <el-form-item label="班级" prop="clazzIds">
          <el-select v-model="addForm.clazzIds" multiple placeholder="请选择">
            <el-option
              v-for="item in ClazzTree"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            ></el-option>
          </el-select>
        </el-form-item>

        <el-form-item label="科目" prop="courseId">
          <el-select v-model="addForm.courseId" placeholder="请选择">
            <el-option
              v-for="item in CourseTree"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            ></el-option>
          </el-select>
        </el-form-item>
      </el-form>
      <div slot="footer" class="dialog-footer">
        <el-button @click.native="addFormVisible = false">取消</el-button>
        <el-button type="primary" @click.native="addSubmit" :loading="addLoading">提交</el-button>
      </div>
    </el-dialog>


    <!--编辑界面-->
    <el-dialog
      title="编辑教师"
      :visible.sync="editFormVisible"
      v-model="editFormVisible"
      :close-on-click-modal="false"
    >
      <el-form :model="editForm" label-width="80px" :rules="editFormRules" ref="editForm">
        <el-form-item label="教师编号" prop="TeacherNo">
          <el-input v-model="editForm.TeacherNo" auto-complete="off"></el-input>
        </el-form-item>
        <el-form-item label="教师" prop="Name">
          <el-input v-model="editForm.Name" auto-complete="off"></el-input>
        </el-form-item>
        <el-form-item label="账号" prop="Account">
          <el-input v-model="editForm.Account" auto-complete="off"></el-input>
        </el-form-item>

        <el-form-item label="年级" prop="gradeId">
          <el-select @change="getClazzTreeFunc"   v-model="editForm.gradeId" placeholder="请选择">
            <el-option
              v-for="item in GradeTree"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            ></el-option>
          </el-select>
        </el-form-item>

        <el-form-item label="班级" prop="clazzIds">
          <el-select v-model="editForm.clazzIds" multiple placeholder="请选择">
            <el-option
              v-for="item in ClazzTree"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            ></el-option>
          </el-select>
        </el-form-item>

        <el-form-item label="科目" prop="courseId">
          <el-select v-model="editForm.courseId" placeholder="请选择">
            <el-option
              v-for="item in CourseTree"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            ></el-option>
          </el-select>
        </el-form-item>
      </el-form>
      <div slot="footer" class="dialog-footer">
        <el-button @click.native="editFormVisible = false">取消</el-button>
        <el-button type="primary" @click.native="editSubmit" :loading="editLoading">提交</el-button>
      </div>
    </el-dialog>
  </section>
</template>

<script>
import util from "../../../util/date";
import {
  addTeacher,
  getTeacherListPage,
  getGradeTree,
  removeTeacher,
  getClazzTree,
  getCourseTree,
  editTeacher
} from "../../api/api";

export default {
  data() {
    return {
      filters: {
        name: ""
      },
      Teacher: [],
      GradeTree:[],
      ClazzTree:[],
      CourseTree:[],
      total: 0,
      page: 1,
      listLoading: false,
      sels: [], //列表选中列
      editFormVisible: false, //编辑界面是否显示
      editLoading: false,
      editFormRules: {
        TeacherNo: [
          { required: true, message: "请输入教师编号", trigger: "blur" }
        ],
        Name: [{ required: true, message: "请输入教师", trigger: "blur" }],
        Account: [{ required: true, message: "请输入账号", trigger: "blur" }]
      },
      //编辑界面数据
      editForm: {
        id: 0,
        TeacherNo: "",
        Name: "",
        Account: "",
        courseId: 0,
        clazzIds: [],
        gradeId: 0
      },
      addFormVisible: false, //编辑界面是否显示
      addLoading: false,
      addFormRules: {
        TeacherNo: [
          { required: true, message: "请输入教师编号", trigger: "blur" }
        ],
        Name: [{ required: true, message: "请输入教师", trigger: "blur" }],
        Account: [{ required: true, message: "请输入账号", trigger: "blur" }]
      },
      //编辑界面数据
      addForm: {
        id: 0,
        TeacherNo: "",
        Name: "",
        Account: "",
        courseId: 0,
        clazzIds: [],
        gradeId: 0
      }
    };
  },
  methods: {
    //性别显示转换
    formatSex: function(row, column) {
      return row.sex == 1 ? "男" : row.sex == 0 ? "女" : "未知";
    },
    formatBirth: function(row, column) {
      return !row.birth || row.birth == ""
        ? ""
        : util.formatDate.format(new Date(row.birth), "yyyy-MM-dd");
    },
    nextPageTeacher(val) {
      this.page = val;
      this.getTeacher();
    },
    //获取用户列表
    getTeacher() {
      let para = {
        page: this.page,
        key: this.filters.name
      };
      this.listLoading = true;

      getTeacherListPage(para).then(res => {
        this.total = res.data.response.dataCount;
        this.Teacher = res.data.response.data;
        this.listLoading = false;
      });
    },
    //删除
    deleteTeacher: function(index, row) {
      this.$confirm("确认删除该记录吗?", "提示", {
        type: "warning"
      })
        .then(() => {
          this.listLoading = true;
          //NProgress.start();
          let para = { id: row.Id };
          removeTeacher(para).then(res => {
            if (util.isEmt.format(res)) {
              this.listLoading = false;
              return;
            }
            this.listLoading = false;
            //NProgress.done();
            if (res.data.success) {
              this.$message({
                message: "删除成功",
                type: "success"
              });
            } else {
              this.$message({
                message: res.data.msg,
                type: "error"
              });
            }

            this.getTeacher();
          });
        })
        .catch(() => {});
    },
    //显示编辑界面
    editTeacher: function(index, row) {
      this.editFormVisible = true;
      this.editForm = Object.assign({}, row);

      
      this.ClazzTree = [];
      let para = { gid: row.gradeId };
      getClazzTree(para).then(res => {
        this.ClazzTree = res.data.response;
      });
    },
    //编辑
    editSubmit: function() {
      this.$refs.editForm.validate(valid => {
        if (valid) {
          this.$confirm("确认提交吗？", "提示", {}).then(() => {
            this.editLoading = true;
            //NProgress.start();
            let para = Object.assign({}, this.editForm);

            editTeacher(para).then(res => {
              if (util.isEmt.format(res)) {
                this.editLoading = false;
                return;
              }
              if (res.data.success) {
                this.editLoading = false;
                //NProgress.done();
                this.$message({
                  message: res.data.msg,
                  type: "success"
                });
                this.$refs["editForm"].resetFields();
                this.editFormVisible = false;
                this.getTeacher();
              } else {
                this.$message({
                  message: res.data.msg,
                  type: "error"
                });
              }
            });
          });
        }
      });
    },
    //显示新增界面
    addTeacher() {
      this.addFormVisible = true;
      this.addForm = {};

      this.ClazzTree = [];

    },
    //新增
    addSubmit: function() {
        
      this.$refs.addForm.validate(valid => {
        if (valid) {
          this.$confirm("确认提交吗？", "提示", {}).then(() => {
            this.addLoading = true;
            let para = Object.assign({}, this.addForm);
            
            addTeacher(para).then(res => {
              if (util.isEmt.format(res)) {
                this.addLoading = false;
                return;
              }

              if (res.data.success) {
                this.addLoading = false;
                //NProgress.done();
                this.$message({
                  message: res.data.msg,
                  type: "success"
                });
                this.$refs["addForm"].resetFields();
                this.addFormVisible = false;
                this.getTeacher();
              } else {
                this.$message({
                  message: res.data.msg,
                  type: "error"
                });
              }
            });
          });
        }
      });
    },
    selsChange: function(sels) {
      this.sels = sels;
    },
    //批量删除
    batchRemove: function() {
      // return;

      var ids = this.sels.map(item => item.Id).toString();
      this.$confirm("确认删除选中记录吗？", "提示", {
        type: "warning"
      })
        .then(() => {
          this.listLoading = true;
          //NProgress.start();
          let para = { ids: ids };

          batchRemoveTeacher(para).then(res => {
            this.listLoading = false;
            //NProgress.done();
            this.$message({
              message: "该功能未开放",
              type: "warning"
            });
            console.log(res);
          });
        })
        .catch(() => {});
    },
    getClazzTreeFunc() {
      
      var gid = this.editForm.gradeId>0?this.editForm.gradeId:this.addForm.gradeId;

      let para = { gid: gid };
      getClazzTree(para).then(res => {
        this.ClazzTree = res.data.response;
      });
    },
  },
  mounted() {
    this.getTeacher();
    let para = {};
    getGradeTree(para).then(res => {
      this.GradeTree = res.data.response;
    });
    getCourseTree(para).then(res => {
      this.CourseTree = res.data.response;
    });
  }
};
</script>

