<template>
  <section>
    <!--工具条-->
    <el-col :span="24" class="toolbar" style="padding-bottom: 0px;">
      <el-form :inline="true" :model="filters" @submit.native.prevent>
        <el-form-item>
          <el-input v-model="filters.name" placeholder="输入查询内容"></el-input>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="getExamDetail">查询</el-button>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="addExamDetail">新增</el-button>
        </el-form-item>
      </el-form>
    </el-col>

    <!--列表-->
    <el-table
      :data="ExamDetail"
      highlight-current-row
      v-loading="listLoading"
      @selection-change="selsChange"
      style="width: 100%;"
    >
      <el-table-column type="selection" width="50"></el-table-column>
      <el-table-column type="index" width="80"></el-table-column>
      <!-- <el-table-column prop="exam.grade.EnrollmentYear" label="入学年份" width sortable></el-table-column> -->
      <el-table-column label="年级" width sortable>
        <template
          scope="scope"
        >{{scope.row.exam.grade.EnrollmentYear+"级"+scope.row.exam.grade.Name}}</template>
      </el-table-column>
      <el-table-column prop="exam.AcademicYear" label="学年" width sortable></el-table-column>
      <el-table-column prop="exam.SchoolTerm" label="学期" width sortable></el-table-column>
      <el-table-column prop="exam.ExamName" label="考试" width sortable></el-table-column>
      <el-table-column prop="exam.course.Name" label="科目" width sortable></el-table-column>
      <el-table-column prop="EDType" label="题型" width sortable></el-table-column>
      <el-table-column prop="Name" label="题目" width sortable></el-table-column>
      <el-table-column prop="Answer" label="正确答案" width sortable></el-table-column>
      <el-table-column prop="Score" label="总分" width sortable></el-table-column>

      <el-table-column label="操作" width="150">
        <template scope="scope">
          <el-button size="small" @click="editExamDetail(scope.$index, scope.row)">编辑</el-button>
          <el-button
            type="danger"
            size="small"
            @click="deleteExamDetail(scope.$index, scope.row)"
          >删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <!--工具条-->
    <el-col :span="24" class="toolbar">
      <el-button type="danger" @click="batchRemove" :disabled="this.sels.length===0">批量删除</el-button>
      <el-pagination
        layout="prev, pager, next"
        @current-change="nextPageExamDetail"
        :page-size="50"
        :total="total"
        style="float:right;"
      ></el-pagination>
    </el-col>

    <!--新增界面-->
    <el-dialog
      title="新增题目"
      :visible.sync="addFormVisible"
      v-model="addFormVisible"
      :close-on-click-modal="false"
    >
      <el-form :model="addForm" label-width="80px" :rules="addFormRules" ref="addForm">
         <el-form-item label="年级" prop="gradeid">
          <el-select @change="getExamTreeFunc" v-model="addForm.gradeid" placeholder="请选择">
            <el-option
              v-for="item in GradeTree"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            ></el-option>
          </el-select>
        </el-form-item>
        <el-form-item label="考试" prop="examid">
          <el-select filterable style="width:100%;" v-model="addForm.examid" placeholder="请选择，可以搜索">
            <el-option
              v-for="item in ExamTree"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            ></el-option>
          </el-select>
        </el-form-item>
        <el-form-item label="题目" prop="Name">
          <el-input v-model="addForm.Name" auto-complete="off"></el-input>
        </el-form-item>
        <el-form-item label="题型" prop="EDType">
          <el-select v-model="addForm.EDType" placeholder="请选择">
            <el-option label="主观题" value="主观题"></el-option>
            <el-option label="客观题" value="客观题"></el-option>
          </el-select>
        </el-form-item>
        <el-form-item label="总分" prop="Score">
          <el-input v-model="addForm.Score" auto-complete="off"></el-input>
        </el-form-item>
      
        <el-form-item label="答案" prop="Answer">
          <el-input v-model="addForm.Answer" auto-complete="off"></el-input>
        </el-form-item>
      </el-form>
      <div slot="footer" class="dialog-footer">
        <el-button @click.native="addFormVisible = false">取消</el-button>
        <el-button type="primary" @click.native="addSubmit" :loading="addLoading">提交</el-button>
      </div>
    </el-dialog>

    <!--编辑界面-->
    <el-dialog
      title="编辑题目"
      :visible.sync="editFormVisible"
      v-model="editFormVisible"
      :close-on-click-modal="false"
    >
      <el-form :model="editForm" label-width="80px" :rules="editFormRules" ref="editForm">
        <el-form-item label="题目" prop="Name">
          <el-input v-model="editForm.Name" auto-complete="off"></el-input>
        </el-form-item>
        <el-form-item label="题型" prop="EDType">
          <el-select v-model="editForm.EDType" placeholder="请选择">
            <el-option label="主观题" value="主观题"></el-option>
            <el-option label="客观题" value="客观题"></el-option>
          </el-select>
        </el-form-item>
        <el-form-item label="总分" prop="Score">
          <el-input v-model="editForm.Score" auto-complete="off"></el-input>
        </el-form-item>
        <el-form-item label="答案" prop="Answer">
          <el-input v-model="editForm.Answer" auto-complete="off"></el-input>
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
  addExamDetail,
  getExamDetailListPage,
  removeExamDetail,
  getGradeTree,
  getExamTree,
  editExamDetail
} from "../../api/api";

export default {
  data() {
    return {
      filters: {
        name: ""
      },
      ExamDetail: [],
      ExamTree: [],
      GradeTree: [],
      total: 0,
      page: 1,
      listLoading: false,
      sels: [], //列表选中列
      editFormVisible: false, //编辑界面是否显示
      editLoading: false,
      editFormRules: {
        Name: [{ required: true, message: "请输入题目", trigger: "blur" }],
        EDType: [{ required: true, message: "请输入题型", trigger: "blur" }],
        Score: [{ required: true, message: "请输入分数", trigger: "blur" }],
      },
      //编辑界面数据
      editForm: {
        Id: 0,
        Name: "",
        EDType: "",
        Score: "",
        Answer: "",
        gradeid:0,
        examid:0,
      },
      addFormVisible: false, //编辑界面是否显示
      addLoading: false,
      addFormRules: {
        Name: [{ required: true, message: "请输入题目", trigger: "blur" }],
        EDType: [{ required: true, message: "请输入题型", trigger: "blur" }],
        Score: [{ required: true, message: "请输入分数", trigger: "blur" }],
      },
      //编辑界面数据
      addForm: {
        Id: 0,
        Name: "",
        EDType: "",
        Score: "",
        Answer: "",
        gradeid:0,
        examid:0,
      }
    };
  },
  methods: {
    getExamTreeFunc() {
      var gid =
        this.editForm.gradeid > 0
          ? this.editForm.gradeid
          : this.addForm.gradeid;

      let para = { gid: gid };
      getExamTree(para).then(res => {
        this.ExamTree = res.data.response;
      });
    },
    //性别显示转换
    formatSex: function(row, column) {
      return row.sex == 1 ? "男" : row.sex == 0 ? "女" : "未知";
    },
    formatBirth: function(row, column) {
      return !row.birth || row.birth == ""
        ? ""
        : util.formatDate.format(new Date(row.birth), "yyyy-MM-dd");
    },
    nextPageExamDetail(val) {
      this.page = val;
      this.getExamDetail();
    },
    //获取用户列表
    getExamDetail() {
      let para = {
        page: this.page,
        key: this.filters.name
      };
      this.listLoading = true;

      getExamDetailListPage(para).then(res => {
        this.total = res.data.response.dataCount;
        this.ExamDetail = res.data.response.data;
        this.listLoading = false;
      });
    },
    //删除
    deleteExamDetail: function(index, row) {
      this.$confirm("确认删除该记录吗?", "提示", {
        type: "warning"
      })
        .then(() => {
          this.listLoading = true;
          //NProgress.start();
          let para = { id: row.Id };
          removeExamDetail(para).then(res => {
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

            this.getExamDetail();
          });
        })
        .catch(() => {});
    },
    //显示编辑界面
    editExamDetail: function(index, row) {
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

            editExamDetail(para).then(res => {
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
                this.getExamDetail();
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
    addExamDetail() {
      this.addFormVisible = true;
      this.addForm = {};
      this.ExamTree = [];

    },
    //新增
    addSubmit: function() {
      this.$refs.addForm.validate(valid => {
        if (valid) {
          this.$confirm("确认提交吗？", "提示", {}).then(() => {
            this.addLoading = true;
            let para = Object.assign({}, this.addForm);
            addExamDetail(para).then(res => {
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
                this.getExamDetail();
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

          batchRemoveExamDetail(para).then(res => {
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
    this.getExamDetail();
    
    let para2 = {};
    getGradeTree(para2).then(res => {
      this.GradeTree = res.data.response;
    });
  }
};
</script>

<style scoped>
</style>
