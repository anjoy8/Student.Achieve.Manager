<template>
  <section>
    <!--工具条-->
    <el-col :span="24" class="toolbar" style="padding-bottom: 0px;">
      <el-form :inline="true" :model="filters" @submit.native.prevent>
        <el-form-item>
          <el-input v-model="filters.name" placeholder="输入查询内容"></el-input>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="getClazz">查询</el-button>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="addClazz">新增</el-button>
        </el-form-item>
      </el-form>
    </el-col>

    <!--列表-->
    <el-table
      :data="Clazz"
      highlight-current-row
      v-loading="listLoading"
      @selection-change="selsChange"
      style="width: 100%;"
    >
      <el-table-column type="selection" width="50"></el-table-column>
      <el-table-column type="index" width="80"></el-table-column>
      <!-- <el-table-column prop="Grade.EnrollmentYear" label="入学年份" width sortable></el-table-column>
      <el-table-column prop="Grade.Name" label="年级" width sortable></el-table-column>-->
      <el-table-column label="年级" width sortable>
        <template scope="scope">{{scope.row.Grade.EnrollmentYear+"级"+scope.row.Grade.Name}}</template>
      </el-table-column>
      <el-table-column prop="ClassNo" label="班级" width sortable></el-table-column>
      <el-table-column prop="ClazzLevel" label="级组" width sortable></el-table-column>
      <el-table-column prop="Manager" label="级长" width sortable></el-table-column>
      <el-table-column prop="ClazzType" label="班类" width sortable></el-table-column>
      <el-table-column prop="TeacherCharge" label="班主任" width sortable></el-table-column>
      <el-table-column prop="ChooseSub" label="选科" width sortable></el-table-column>

      <el-table-column label="操作" width="150">
        <template scope="scope">
          <el-button size="small" @click="editClazz(scope.$index, scope.row)">编辑</el-button>
          <el-button type="danger" size="small" @click="deleteClazz(scope.$index, scope.row)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <!--工具条-->
    <el-col :span="24" class="toolbar">
      <el-button type="danger" @click="batchRemove" :disabled="this.sels.length===0">批量删除</el-button>
      <el-pagination
        layout="prev, pager, next"
        @current-change="nextPageClazz"
        :page-size="50"
        :total="total"
        style="float:right;"
      ></el-pagination>
    </el-col>

    <!--新增界面-->
    <el-dialog
      title="新增班级"
      :visible.sync="addFormVisible"
      v-model="addFormVisible"
      :close-on-click-modal="false"
    >
      <el-form :model="addForm" label-width="80px" :rules="addFormRules" ref="addForm">
        <el-form-item label="班级" prop="ClassNo">
          <el-input v-model="addForm.ClassNo" auto-complete="off"></el-input>
        </el-form-item>
        <el-form-item label="年级" prop="GradeId">
          <el-select v-model="addForm.GradeId" placeholder="请选择">
            <el-option
              v-for="item in GradeTree"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            ></el-option>
          </el-select>
        </el-form-item>

        <el-form-item label="级组" prop="ClazzLevel">
          <el-input v-model="addForm.ClazzLevel" auto-complete="off"></el-input>
        </el-form-item>
        <el-form-item label="级长" prop="Manager">
          <el-input v-model="addForm.Manager" auto-complete="off"></el-input>
        </el-form-item>
        <el-form-item label="班类" prop="ClazzType">
          <el-input v-model="addForm.ClazzType" auto-complete="off"></el-input>
        </el-form-item>
        <el-form-item label="班主任" prop="TeacherCharge">
          <el-input v-model="addForm.TeacherCharge" auto-complete="off"></el-input>
        </el-form-item>
        <el-form-item label="选科" prop="ChooseSub">
          <el-input v-model="addForm.ChooseSub" auto-complete="off"></el-input>
        </el-form-item>
      </el-form>
      <div slot="footer" class="dialog-footer">
        <el-button @click.native="addFormVisible = false">取消</el-button>
        <el-button type="primary" @click.native="addSubmit" :loading="addLoading">提交</el-button>
      </div>
    </el-dialog>

    <!--编辑界面-->
    <el-dialog
      title="编辑班级"
      :visible.sync="editFormVisible"
      v-model="editFormVisible"
      :close-on-click-modal="false"
    >
      <el-form :model="editForm" label-width="80px" :rules="editFormRules" ref="editForm">
        <el-form-item label="班级" prop="ClassNo">
          <el-input v-model="editForm.ClassNo" auto-complete="off"></el-input>
        </el-form-item>
        <el-form-item label="年级" prop="GradeId">
          <el-select v-model="editForm.GradeId" placeholder="请选择">
            <el-option
              v-for="item in GradeTree"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            ></el-option>
          </el-select>
        </el-form-item>

        <el-form-item label="级组" prop="ClazzLevel">
          <el-input v-model="editForm.ClazzLevel" auto-complete="off"></el-input>
        </el-form-item>
        <el-form-item label="级长" prop="Manager">
          <el-input v-model="editForm.Manager" auto-complete="off"></el-input>
        </el-form-item>
        <el-form-item label="班类" prop="ClazzType">
          <el-input v-model="editForm.ClazzType" auto-complete="off"></el-input>
        </el-form-item>
        <el-form-item label="班主任" prop="TeacherCharge">
          <el-input v-model="editForm.TeacherCharge" auto-complete="off"></el-input>
        </el-form-item>
        <el-form-item label="选科" prop="ChooseSub">
          <el-input v-model="editForm.ChooseSub" auto-complete="off"></el-input>
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
  addClazz,
  getClazzListPage,
  removeClazz,
  editClazz,
  getGradeTree
} from "../../api/api";

export default {
  data() {
    return {
      filters: {
        name: ""
      },
      Clazz: [],
      total: 0,
      page: 1,
      GradeTree: [],
      listLoading: false,
      sels: [], //列表选中列
      editFormVisible: false, //编辑界面是否显示
      editLoading: false,
      editFormRules: {
        ClassNo: [{ required: true, message: "请输入班级名", trigger: "blur" }],
        ClazzLevel: [
          { required: true, message: "请输入级组", trigger: "blur" }
        ],
        Manager: [{ required: true, message: "请输入级长", trigger: "blur" }],
        ClazzType: [{ required: true, message: "请输入班类", trigger: "blur" }],
        TeacherCharge: [
          { required: true, message: "请输入班主任", trigger: "blur" }
        ],
        ChooseSub: [{ required: true, message: "请输入选科", trigger: "blur" }]
      },
      //编辑界面数据
      editForm: {
        Id: 0,
        ClassNo: "",
        ClazzLevel: "",
        Manager: "",
        ClazzType: "",
        TeacherCharge: "",
        ChooseSub: ""
      },
      addFormVisible: false, //编辑界面是否显示
      addLoading: false,
      addFormRules: {
        ClassNo: [{ required: true, message: "请输入班级名", trigger: "blur" }],
        ClazzLevel: [
          { required: true, message: "请输入级组", trigger: "blur" }
        ],
        Manager: [{ required: true, message: "请输入级长", trigger: "blur" }],
        ClazzType: [{ required: true, message: "请输入班类", trigger: "blur" }],
        TeacherCharge: [
          { required: true, message: "请输入班主任", trigger: "blur" }
        ],
        ChooseSub: [{ required: true, message: "请输入选科", trigger: "blur" }]
      },
      //编辑界面数据
      addForm: {
        Id: 0,
        ClassNo: "",
        ClazzLevel: "",
        Manager: "",
        ClazzType: "",
        TeacherCharge: "",
        ChooseSub: ""
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
    nextPageClazz(val) {
      this.page = val;
      this.getClazz();
    },
    //获取用户列表
    getClazz() {
      let para = {
        page: this.page,
        key: this.filters.name
      };
      this.listLoading = true;

      getClazzListPage(para).then(res => {
        this.total = res.data.response.dataCount;
        this.Clazz = res.data.response.data;
        this.listLoading = false;
      });
    },
    //删除
    deleteClazz: function(index, row) {
      this.$confirm("确认删除该记录吗?", "提示", {
        type: "warning"
      })
        .then(() => {
          this.listLoading = true;
          //NProgress.start();
          let para = { id: row.Id };
          removeClazz(para).then(res => {
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

            this.getClazz();
          });
        })
        .catch(() => {});
    },
    //显示编辑界面
    editClazz: function(index, row) {
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

            editClazz(para).then(res => {
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
                this.getClazz();
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
    addClazz() {
      this.addFormVisible = true;
      this.addForm = {};
    },
    //新增
    addSubmit: function() {
      this.$refs.addForm.validate(valid => {
        if (valid) {
          this.$confirm("确认提交吗？", "提示", {}).then(() => {
            this.addLoading = true;
            this.addForm.Name = this.addForm.ClassNo;
            let para = Object.assign({}, this.addForm);
            addClazz(para).then(res => {
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
                this.getClazz();
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

          batchRemoveClazz(para).then(res => {
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
    this.getClazz();

    let para = {};
    getGradeTree(para).then(res => {
      this.GradeTree = res.data.response;
    });
  }
};
</script>

<style scoped>
</style>
