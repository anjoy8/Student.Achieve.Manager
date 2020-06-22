<template>
  <section>
    <!--工具条-->
    <el-col :span="24" class="toolbar" style="padding-bottom: 0px;">
      <el-form :inline="true" :model="filters" @submit.native.prevent>
        <el-form-item>
          <el-input v-model="filters.name" placeholder="输入查询内容"></el-input>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="getExam">查询</el-button>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="addExam">新增</el-button>
        </el-form-item>
      </el-form>
    </el-col>

    <!--列表-->
    <el-table
      :data="Exam"
      highlight-current-row
      v-loading="listLoading"
      @selection-change="selsChange"
      style="width: 100%;"
    >
      <el-table-column type="selection" width="50"></el-table-column>
      <el-table-column type="index" width="80"></el-table-column>
      <!-- <el-table-column prop="grade.Name" label="年级" width sortable></el-table-column> -->
       <el-table-column label="年级" width sortable>
        <template
          scope="scope"
        >{{scope.row.grade.EnrollmentYear+"级"+scope.row.grade.Name}}</template>
      </el-table-column>
      <el-table-column prop="AcademicYear" label="学年" width sortable></el-table-column>
      <el-table-column prop="SchoolTerm" label="学期" width sortable></el-table-column>
      <el-table-column prop="ExamName" label="考试" width sortable></el-table-column>
      <el-table-column prop="course.Name" label="考试科目" width sortable></el-table-column>

      <el-table-column label="操作" width="150">
        <template scope="scope">
          <el-button size="small" @click="editExam(scope.$index, scope.row)">编辑</el-button>
          <el-button type="danger" size="small" @click="deleteExam(scope.$index, scope.row)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <!--工具条-->
    <el-col :span="24" class="toolbar">
      <el-button type="danger" @click="batchRemove" :disabled="this.sels.length===0">批量删除</el-button>
      <el-pagination
        layout="prev, pager, next"
        @current-change="nextPageExam"
        :page-size="50"
        :total="total"
        style="float:right;"
      ></el-pagination>
    </el-col>

    <!--新增界面-->
    <el-dialog
      title="新增考试"
      :visible.sync="addFormVisible"
      v-model="addFormVisible"
      :close-on-click-modal="false"
    >
      <el-form :model="addForm" label-width="80px" :rules="addFormRules" ref="addForm">
        <el-form-item label="学年" prop="AcademicYear">
          <el-input v-model="addForm.AcademicYear" auto-complete="off"></el-input>
        </el-form-item>
        <el-form-item label="学期" prop="SchoolTerm">
          <el-input v-model="addForm.SchoolTerm" auto-complete="off"></el-input>
        </el-form-item>
        <el-form-item label="考试" prop="ExamName">
          <el-input v-model="addForm.ExamName" auto-complete="off"></el-input>
        </el-form-item>
        
        <el-form-item label="年级" prop="gradeid">
          <el-select v-model="addForm.gradeid" placeholder="请选择">
            <el-option
              v-for="item in GradeTree"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            ></el-option>
          </el-select>
        </el-form-item>
        <el-form-item label="科目" prop="courseid">
          <el-select v-model="addForm.courseid" placeholder="请选择">
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
      title="编辑考试"
      :visible.sync="editFormVisible"
      v-model="editFormVisible"
      :close-on-click-modal="false"
    >
      <el-form :model="editForm" label-width="80px" :rules="editFormRules" ref="editForm">
        <el-form-item label="学年" prop="AcademicYear">
          <el-input v-model="editForm.AcademicYear" auto-complete="off"></el-input>
        </el-form-item>
        <el-form-item label="学期" prop="SchoolTerm">
          <el-input v-model="editForm.SchoolTerm" auto-complete="off"></el-input>
        </el-form-item>
        <el-form-item label="考试" prop="ExamName">
          <el-input v-model="editForm.ExamName" auto-complete="off"></el-input>
        </el-form-item>

        <el-form-item label="年级" prop="gradeid">
          <el-select v-model="editForm.gradeid" placeholder="请选择">
            <el-option
              v-for="item in GradeTree"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            ></el-option>
          </el-select>
        </el-form-item>
        <el-form-item label="科目" prop="courseid">
          <el-select v-model="editForm.courseid" placeholder="请选择">
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
import { addExam, getExamListPage, removeExam, editExam,getCourseTree,getGradeTree } from "../../api/api";

export default {
  data() {
    return {
      filters: {
        name: ""
      },
      Exam: [],
      GradeTree: [],
      CourseTree: [],
      total: 0,
      page: 1,
      listLoading: false,
      sels: [], //列表选中列
      editFormVisible: false, //编辑界面是否显示
      editLoading: false,
      editFormRules: {
        AcademicYear: [
          { required: true, message: "请输入学年", trigger: "blur" }
        ],
        SchoolTerm: [
          { required: true, message: "请输入学期", trigger: "blur" }
        ],
        ExamName: [{ required: true, message: "请输入考试", trigger: "blur" }]
      },
      //编辑界面数据
      editForm: {
        Id: 0,
        AcademicYear: "",
        ExamName: "",
        SchoolTer: "",
        gradeid: 0,
        courseid: 0
      },
      addFormVisible: false, //编辑界面是否显示
      addLoading: false,
      addFormRules: {
        AcademicYear: [
          { required: true, message: "请输入学年", trigger: "blur" }
        ],
        SchoolTerm: [
          { required: true, message: "请输入学期", trigger: "blur" }
        ],
        ExamName: [{ required: true, message: "请输入考试", trigger: "blur" }]
      },
      //编辑界面数据
      addForm: {
        Id: 0,
        AcademicYear: "",
        ExamName: "",
        SchoolTer: "",
        gradeid: 0,
        courseid: 0
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
    nextPageExam(val) {
      this.page = val;
      this.getExam();
    },
    //获取用户列表
    getExam() {
      let para = {
        page: this.page,
        key: this.filters.name
      };
      this.listLoading = true;

      getExamListPage(para).then(res => {
        this.total = res.data.response.dataCount;
        this.Exam = res.data.response.data;
        this.listLoading = false;
      });
    },
    //删除
    deleteExam: function(index, row) {
      this.$confirm("确认删除该记录吗?", "提示", {
        type: "warning"
      })
        .then(() => {
          this.listLoading = true;
          //NProgress.start();
          let para = { id: row.Id };
          removeExam(para).then(res => {
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

            this.getExam();
          });
        })
        .catch(() => {});
    },
    //显示编辑界面
    editExam: function(index, row) {
      this.editFormVisible = true;
      this.editForm = Object.assign({}, row);
    },
    //编辑
    editSubmit: function() {
      this.$refs.editForm.validate(valid => {
        if (valid) {
          this.$confirm("确认提交吗？", "提示", {}).then(() => {
            this.editLoading = true;
            //NProgress.start();
            let para = Object.assign({}, this.editForm);

            editExam(para).then(res => {
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
                this.getExam();
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
    addExam() {
      this.addFormVisible = true;
      this.addForm = {};
    },
    //新增
    addSubmit: function() {
      this.$refs.addForm.validate(valid => {
        if (valid) {
          this.$confirm("确认提交吗？", "提示", {}).then(() => {
            this.addLoading = true;
            let para = Object.assign({}, this.addForm);
            addExam(para).then(res => {
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
                this.getExam();
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

          batchRemoveExam(para).then(res => {
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
    }
  },
  mounted() {
    this.getExam();
    let para = {};
    getGradeTree(para).then(res => {
      this.GradeTree = res.data.response;
    });

    let para2 = {};
    getCourseTree(para2).then(res => {
      this.CourseTree = res.data.response;
    });
  }
};
</script>

<style scoped>
</style>
