<template>
  <section>
    <!--工具条-->
    <el-col :span="24" class="toolbar" style="padding-bottom: 0px;">
      <el-form :inline="true" :model="filters" @submit.native.prevent>
        <el-form-item>
          <el-input v-model="filters.name" placeholder="输入查询内容"></el-input>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="getExamDetailScore">查询</el-button>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="addExamDetailScore">新增</el-button>
        </el-form-item>
      </el-form>
    </el-col>

    <!--列表-->
    <el-table
      :data="ExamDetailScore"
      highlight-current-row
      v-loading="listLoading"
      @selection-change="selsChange"
      style="width: 100%;"
    >
      <el-table-column type="selection" width="50"></el-table-column>
      <el-table-column type="index" width="80"></el-table-column>
      <el-table-column label="年级" width sortable>
        <template
          scope="scope"
        >{{scope.row.ExamDetail.exam.grade.EnrollmentYear+"级"+scope.row.ExamDetail.exam.grade.Name}}</template>
      </el-table-column>
      <el-table-column prop="ExamDetail.exam.AcademicYear" label="学年" width sortable></el-table-column>
      <el-table-column prop="ExamDetail.exam.SchoolTerm" label="学期" width sortable></el-table-column>
      <el-table-column prop="ExamDetail.exam.ExamName" label="考试" width sortable></el-table-column>
      <el-table-column prop="ExamDetail.exam.course.Name" label="科目" width sortable></el-table-column>
      <el-table-column prop="ExamDetail.Name" label="题目" width sortable></el-table-column>
      <el-table-column prop="student.Name" label="考生" width sortable></el-table-column>
      <el-table-column prop="StudentAnswer" label="考生答案" width sortable></el-table-column>
      <el-table-column prop="StudentScore" label="考生得分" width sortable></el-table-column>

      <el-table-column label="操作" width="150">
        <template scope="scope">
          <el-button size="small" @click="editExamDetailScore(scope.$index, scope.row)">编辑</el-button>
          <el-button
            type="danger"
            size="small"
            @click="deleteExamDetailScore(scope.$index, scope.row)"
          >删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <!--工具条-->
    <el-col :span="24" class="toolbar">
      <el-button type="danger" @click="batchRemove" :disabled="this.sels.length===0">批量删除</el-button>
      <el-pagination
        layout="prev, pager, next"
        @current-change="nextPageExamDetailScore"
        :page-size="50"
        :total="total"
        style="float:right;"
      ></el-pagination>
    </el-col>

    <!--新增界面-->
    <el-dialog
      title="新增题目得分"
      :visible.sync="addFormVisible"
      v-model="addFormVisible"
      :close-on-click-modal="false"
    >
      <el-form :model="addForm" label-width="80px" :rules="addFormRules" ref="addForm">
        <el-form-item label="考生答案" prop="StudentAnswer">
          <el-input v-model="addForm.StudentAnswer" auto-complete="off"></el-input>
        </el-form-item>
        <el-form-item label="考试得分" prop="StudentScore">
          <el-input v-model="addForm.StudentScore" auto-complete="off"></el-input>
        </el-form-item>
      </el-form>
      <div slot="footer" class="dialog-footer">
        <el-button @click.native="addFormVisible = false">取消</el-button>
        <el-button type="primary" @click.native="addSubmit" :loading="addLoading">提交</el-button>
      </div>
    </el-dialog>

    <!--编辑界面-->
    <el-dialog
      title="编辑题目得分"
      :visible.sync="editFormVisible"
      v-model="editFormVisible"
      :close-on-click-modal="false"
    >
      <el-form :model="editForm" label-width="80px" :rules="editFormRules" ref="editForm">
       <el-form-item label="考生答案" prop="StudentAnswer">
          <el-input v-model="editForm.StudentAnswer" auto-complete="off"></el-input>
        </el-form-item>
        <el-form-item label="考试得分" prop="StudentScore">
          <el-input v-model="editForm.StudentScore" auto-complete="off"></el-input>
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
  addExamDetailScore,
  getExamDetailScoreListPage,
  removeExamDetailScore,
  editExamDetailScore
} from "../../api/api";

export default {
  data() {
    return {
      filters: {
        name: ""
      },
      ExamDetailScore: [],
      total: 0,
      page: 1,
      listLoading: false,
      sels: [], //列表选中列
      editFormVisible: false, //编辑界面是否显示
      editLoading: false,
      editFormRules: {
        StudentAnswer: [
          { required: true, message: "请输入考生答案", trigger: "blur" }
        ],
        StudentScore: [{ required: true, message: "请输入考生得分", trigger: "blur" }]
      },
      //编辑界面数据
      editForm: {
        Id: 0,
        StudentAnswer: "",
        StudentScore: ""
      },
      addFormVisible: false, //编辑界面是否显示
      addLoading: false,
      addFormRules: {
        StudentAnswer: [
          { required: true, message: "请输入考生答案", trigger: "blur" }
        ],
        StudentScore: [{ required: true, message: "请输入考生得分", trigger: "blur" }]
      },
      //编辑界面数据
      addForm: {
        Id: 0,
        StudentAnswer: "",
        StudentScore: ""
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
    nextPageExamDetailScore(val) {
      this.page = val;
      this.getExamDetailScore();
    },
    //获取用户列表
    getExamDetailScore() {
      let para = {
        page: this.page,
        key: this.filters.name
      };
      this.listLoading = true;

      getExamDetailScoreListPage(para).then(res => {
        this.total = res.data.response.dataCount;
        this.ExamDetailScore = res.data.response.data;
        this.listLoading = false;
      });
    },
    //删除
    deleteExamDetailScore: function(index, row) {
      this.$confirm("确认删除该记录吗?", "提示", {
        type: "warning"
      })
        .then(() => {
          this.listLoading = true;
          //NProgress.start();
          let para = { id: row.Id };
          removeExamDetailScore(para).then(res => {
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

            this.getExamDetailScore();
          });
        })
        .catch(() => {});
    },
    //显示编辑界面
    editExamDetailScore: function(index, row) {
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

            editExamDetailScore(para).then(res => {
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
                this.getExamDetailScore();
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
    addExamDetailScore() {
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
            addExamDetailScore(para).then(res => {
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
                this.getExamDetailScore();
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

          batchRemoveExamDetailScore(para).then(res => {
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
    this.getExamDetailScore();
  }
};
</script>

<style scoped>
</style>
